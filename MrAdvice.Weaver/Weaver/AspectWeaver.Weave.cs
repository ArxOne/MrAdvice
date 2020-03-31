#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Advice;
    using Annotation;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using dnlib.DotNet.Pdb;
    using Introduction;
    using Reflection;
    using Reflection.Groups;
    using Utility;
    using FieldAttributes = dnlib.DotNet.FieldAttributes;
    using MethodAttributes = dnlib.DotNet.MethodAttributes;
    using TypeAttributes = dnlib.DotNet.TypeAttributes;

    partial class AspectWeaver
    {
        private const string ShortcutTypeNamespace = "ArxOne.MrAdvice";
        private const string ShortcutTypeName = "\u26A1Invocation";

        /// <summary>
        /// Weaves the info advices for the given type.
        /// </summary>
        /// <param name="infoAdvisedType">Type of the module.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="useWholeAssembly">if set to <c>true</c> [use whole assembly].</param>
        private void WeaveInfoAdvices(TypeDef infoAdvisedType, ModuleDef moduleDefinition, bool useWholeAssembly)
        {
            var invocationType = TypeResolver.Resolve(moduleDefinition, typeof(Invocation));
            if (invocationType == null)
                return;
            var proceedRuntimeInitializersReference = (from m in invocationType.Methods
                                                       where m.IsStatic && m.Name == nameof(Invocation.ProcessInfoAdvices)
                                                       let parameters = m.Parameters
                                                       where parameters.Count == 1
                                                             && parameters[0].Type.SafeEquivalent(
                                                                 moduleDefinition.SafeImport(useWholeAssembly ? typeof(Assembly) : typeof(Type)).ToTypeSig())
                                                       select m).SingleOrDefault();
            if (proceedRuntimeInitializersReference == null)
            {
                Logging.WriteWarning("Info advice method not found");
                return;
            }

            var instructions = GetCctorInstructions(infoAdvisedType);
            var proceedMethod = moduleDefinition.SafeImport(proceedRuntimeInitializersReference);

            if (useWholeAssembly)
                instructions.Emit(OpCodes.Call, moduleDefinition.SafeImport(ReflectionUtility.GetMethodInfo(() => Assembly.GetExecutingAssembly())));
            else
            {
                instructions.Emit(OpCodes.Ldtoken, TypeImporter.Import(moduleDefinition, infoAdvisedType.ToTypeSig()));
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                var getTypeFromHandleMethodInfo = ReflectionUtility.GetMethodInfo(() => Type.GetTypeFromHandle(new RuntimeTypeHandle()));
                instructions.Emit(OpCodes.Call, moduleDefinition.SafeImport(getTypeFromHandleMethodInfo));
            }
            instructions.Emit(OpCodes.Call, proceedMethod);
        }

        /// <summary>
        /// Returns a <see cref="Instructions"/> allowing to insert code in type .cctor.
        /// </summary>
        /// <param name="typeDef">The type definition.</param>
        /// <returns></returns>
        private Instructions GetCctorInstructions(TypeDef typeDef)
        {
            var moduleDefinition = typeDef.Module;
            const string cctorMethodName = ".cctor";
            var staticCtor = typeDef.Methods.SingleOrDefault(m => m.Name == cctorMethodName);
            if (staticCtor == null)
            {
                // the cctor needs to be called after all initialization (in case some info advices collect data)
                typeDef.Attributes &= ~TypeAttributes.BeforeFieldInit;
                var methodAttributes = (InjectAsPrivate ? MethodAttributes.Private : MethodAttributes.Public)
                                       | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                staticCtor =
                    new MethodDefUser(cctorMethodName, MethodSig.CreateStatic(moduleDefinition.CorLibTypes.Void), methodAttributes) { Body = new CilBody() };
                typeDef.Methods.Add(staticCtor);
                staticCtor.Body.Instructions.Add(new Instruction(OpCodes.Ret));
            }

            return new Instructions(staticCtor.Body, staticCtor.Module);
        }

        /// <summary>
        /// Weaves the specified method.
        /// </summary>
        /// <param name="markedMethod">The marked method.</param>
        /// <param name="context">The context.</param>
        private void WeaveAdvices(MarkedNode markedMethod, WeavingContext context)
        {
            var method = markedMethod.Node.Method;

            // sanity check
            if (!method.HasReturnType)
            {
                var customAttributes = method.CustomAttributes;
                if (customAttributes.Any(c => c.AttributeType.Name == "AsyncStateMachineAttribute"))
                    Logging.WriteWarning("Advising async void method '{0}' could confuse async advices. Consider switching its return type to async Task.", method.FullName);
            }

            if (method.IsAbstract)
            {
                method.Attributes = (method.Attributes & ~MethodAttributes.Abstract) | MethodAttributes.Virtual;
                Logging.WriteDebug("Weaving abstract method '{0}'", method.FullName);
                WritePointcutBody(method, null, false, context);
            }
            else if (markedMethod.AbstractTarget)
            {
                Logging.WriteDebug("Weaving and abstracting method '{0}'", method.FullName);
                WritePointcutBody(method, null, true, context);
            }
            else
            {
                Logging.WriteDebug("Weaving method '{0}'", method.FullName);

                var methodName = method.Name;

                // create inner method
                const MethodAttributes attributesToKeep = MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.PInvokeImpl |
                                                          MethodAttributes.UnmanagedExport | MethodAttributes.HasSecurity |
                                                          MethodAttributes.RequireSecObject;
                var innerMethodAttributes = method.Attributes & attributesToKeep |
                                            (InjectAsPrivate ? MethodAttributes.Private : MethodAttributes.Public);
                string innerMethodName;
                if (method.IsGetter)
                    innerMethodName = GetPropertyInnerGetterName(GetSpecialOwnerName(methodName));
                else if (method.IsSetter)
                    innerMethodName = GetPropertyInnerSetterName(GetSpecialOwnerName(methodName));
                else if (method.IsAddOn)
                    innerMethodName = GetEventInnerAdderName(GetSpecialOwnerName(methodName));
                else if (method.IsRemoveOn)
                    innerMethodName = GetEventInnerRemoverName(GetSpecialOwnerName(methodName));
                else
                    innerMethodName = GetInnerMethodName(methodName);
                var innerMethod = new MethodDefUser(innerMethodName, method.MethodSig, innerMethodAttributes);
                new MethodParameters(method).SetParamDefs(innerMethod);
                innerMethod.GenericParameters.AddRange(method.GenericParameters.Select(p => p.Clone(innerMethod)));
                innerMethod.ImplAttributes = method.ImplAttributes;
                innerMethod.SemanticsAttributes = method.SemanticsAttributes;
                if (method.IsPinvokeImpl)
                {
                    innerMethod.ImplMap = method.ImplMap;
                    method.ImplMap = null;
                    method.IsPreserveSig = false;
                    method.IsPinvokeImpl = false;
                }
                else
                {
                    innerMethod.Body = method.Body;
                    method.Body = new CilBody();
                }

                AddGeneratedAttribute(innerMethod, context);

                lock (method.DeclaringType)
                    method.DeclaringType.Methods.Add(innerMethod);

                var stepInfos = innerMethod.CustomDebugInfos.OfType<PdbAsyncMethodCustomDebugInfo>().SelectMany(d => d.StepInfos).ToArray();
                for (var stepInfoIndex = 0; stepInfoIndex < stepInfos.Length; stepInfoIndex++)
                {
                    var stepInfo = stepInfos[stepInfoIndex];
                    Logging.WriteDebug("Found stepInfo for '{0}'", stepInfo.BreakpointMethod.ToString());
                    if (stepInfo.BreakpointMethod.SafeEquivalent(method))
                    {
                        Logging.WriteDebug("Replacing '{0}' with '{1}'", stepInfo.BreakpointMethod.ToString(), innerMethod.ToString());
                        stepInfo.BreakpointMethod = innerMethod;
                        stepInfos[stepInfoIndex] = stepInfo;
                    }
                }

                WritePointcutBody(method, innerMethod, false, context);
            }
        }

        private static MethodDef WriteDelegateProceeder(MethodDef innerMethod, string methodName, MethodParameters parametersList, ModuleDef module)
        {
            if (innerMethod == null)
                return null;
            // currently, this is unsupported
            // (since I have no idea how it works)
            if (innerMethod.DeclaringType.HasGenericParameters || innerMethod.HasGenericParameters)
                return null;

            var proceederMethodSignature = new MethodSig(CallingConvention.Default, 0, module.CorLibTypes.Object,
                new TypeSig[] { module.CorLibTypes.Object, new SZArraySig(module.CorLibTypes.Object) });
            var proceederMethodAttributes = MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig;
            var proceederMethod = new MethodDefUser(GetDelegateProceederName(methodName, innerMethod.DeclaringType), proceederMethodSignature, proceederMethodAttributes);
            proceederMethod.Body = new CilBody();
            proceederMethod.GenericParameters.AddRange(innerMethod.GenericParameters.Select(p => p.Clone(innerMethod)));

            // object, object[] -> this, arguments
            var instructions = new Instructions(proceederMethod.Body, module);

            var declaringType = innerMethod.DeclaringType.ToTypeSig();
            if (innerMethod.DeclaringType.HasGenericParameters)
            {
                var genericTypeArgs = new List<TypeSig>();
                for (int genericTypeArgIndex = 0; genericTypeArgIndex < innerMethod.DeclaringType.GenericParameters.Count; genericTypeArgIndex++)
                    genericTypeArgs.Add(new GenericVar(genericTypeArgIndex, innerMethod.DeclaringType));
                declaringType = new GenericInstSig((ClassOrValueTypeSig)innerMethod.DeclaringType.ToTypeSig(), genericTypeArgs);
            }

            if (!innerMethod.IsStatic)
            {
                instructions.Emit(OpCodes.Ldarg_0);
                if (declaringType.IsValueType)
                    instructions.Emit(OpCodes.Unbox, declaringType); // arg.0 --> (target type) arg.0
                else
                    instructions.Emit(OpCodes.Castclass, declaringType); // arg.0 --> (target type) arg.0
            }
            //instructions.Emit(OpCodes.Ldnull);

            var localVariables = new Local[innerMethod.Parameters.Count];
            for (int parameterIndex = 0; parameterIndex < parametersList.Count; parameterIndex++)
            {
                var parameter = parametersList[parameterIndex];

                if (parameter.ParamDef == null)
                    parameter.CreateParamDef();

                var parameterType = parameter.Type;
                Local local = null;
                // the local type for references is the dereferenced type
                if (parameterType is ByRefSig)
                {
                    parameterType = parameterType.Next;
                    localVariables[parameterIndex] = local = new Local(parameterType);
                    proceederMethod.Body.Variables.Add(local);
                }

                // on pure out values we don't care
                if (!parameter.ParamDef.IsOut)
                {
                    instructions.Emit(OpCodes.Ldarg_1); // arguments[]
                    instructions.EmitLdc(parameterIndex); // index
                    instructions.Emit(OpCodes.Ldelem_Ref); // get array object
                    instructions.EmitUnboxOrCastIfNecessary(parameterType);

                    // when there is a local, use it (because we're going to pass the reference)
                    if (local != null)
                        instructions.EmitStloc(local);
                }
                // in all cases, if there is a local, it means we use it
                if (local != null)
                    instructions.Emit(OpCodes.Ldloca_S, local);
            }

            if (proceederMethod.HasGenericParameters)
            {
                var genericArgs = new List<TypeSig>();
                for (int genericParameterIndex = 0; genericParameterIndex < proceederMethod.GenericParameters.Count; genericParameterIndex++)
                    genericArgs.Add(new GenericMVar(genericParameterIndex, innerMethod));
                var genericInnerMethod = new MethodSpecUser(innerMethod, new GenericInstMethodSig(genericArgs));
                instructions.Emit(OpCodes.Call, genericInnerMethod);
            }
            else
                instructions.Emit(OpCodes.Call, innerMethod);

            // collect ref/output parameters, if any
            for (int parameterIndex = 0; parameterIndex < innerMethod.Parameters.Count; parameterIndex++)
            {
                // when there is a local variable, it was either a ref or an out, so we need to box it again to array
                var localVariable = localVariables[parameterIndex];
                if (localVariable == null)
                    continue;
                instructions.Emit(OpCodes.Ldarg_1); // array[...]
                instructions.EmitLdc(parameterIndex); // index
                instructions.EmitLdloc(localVariable); // result
                instructions.EmitBoxIfNecessary(localVariable.Type); // box
                instructions.Emit(OpCodes.Stelem_Ref); // and store
            }

            if (!innerMethod.HasReturnType)
                instructions.Emit(OpCodes.Ldnull);
            else
                instructions.EmitBoxIfNecessary(innerMethod.ReturnType);

            instructions.Emit(OpCodes.Ret);

            innerMethod.DeclaringType.Methods.Add(proceederMethod);
            return proceederMethod;
        }

        private static void AddGeneratedAttribute(MethodDefUser innerMethod, WeavingContext context)
        {
            // does this happen? Not sure.
            if (context.ExecutionPointAttributeDefaultCtor == null)
                return;
            var generatedAttribute = new CustomAttribute(context.ExecutionPointAttributeDefaultCtor);
            innerMethod.CustomAttributes.Add(generatedAttribute);
        }

        /// <summary>
        /// Weaves method with weaving advices <see cref="IWeavingAdvice"/>.
        /// </summary>
        /// <param name="markedMethod">The marked method.</param>
        /// <param name="context">The context.</param>
        private void RunWeavingAdvices(MarkedNode markedMethod, WeavingContext context)
        {
            var method = markedMethod.Node.Method;
            var methodName = method.Name;

            // our special recipe, with weaving advices
            var weavingAdvicesMarkers = GetAllMarkers(markedMethod.Node, context.WeavingAdviceInterfaceType, context).Select(t => t.Item2).ToArray();
            var typeDefinition = markedMethod.Node.Method.DeclaringType;
            var initialType = TypeLoader.GetType(typeDefinition);
            var weaverMethodWeavingContext = new WeaverMethodWeavingContext(typeDefinition, initialType, methodName, context, TypeResolver, Logging);
            foreach (var weavingAdviceMarker in weavingAdvicesMarkers)
            {
                Logging.WriteDebug("Weaving method '{0}' using weaving advice '{1}'", method.FullName, weavingAdviceMarker.Type.FullName);
                var weavingAdviceType = TypeLoader.GetType(weavingAdviceMarker.Type);
                var weavingAdvice = (IWeavingAdvice)Activator.CreateInstance(weavingAdviceType);
                if (weavingAdvice is IMethodWeavingAdvice methodWeavingAdvice && !method.IsGetter && !method.IsSetter)
                    methodWeavingAdvice.Advise(weaverMethodWeavingContext);
            }
            if (weaverMethodWeavingContext.TargetMethodName != methodName)
                method.Name = weaverMethodWeavingContext.TargetMethodName;
        }

        /// <summary>
        /// Writes the pointcut body.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="innerMethod">The inner method.</param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        private void WritePointcutBody(MethodDef method, MethodDef innerMethod, bool abstractedTarget, WeavingContext context)
        {
            // now empty the old one and make it call the inner method...
            if (method.Body == null)
                method.Body = new CilBody();
            method.Body.InitLocals = true;
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
            method.Body.ExceptionHandlers.Clear();
            var instructions = new Instructions(method.Body, method.Module);

            var targetArgument = GetTargetArgument(method, out var backCopy);
            var parametersArgument = GetParametersArgument(method, out var parametersVariable);
            var methodArgument = GetMethodArgument(method);
            var innerMethodArgument = GetInnerMethodArgument(innerMethod);
            var typeArgument = GetTypeArgument(method);
            var abstractedArgument = GetAbstractedArgument(abstractedTarget);
            var genericParametersArgument = GetGenericParametersArgument(method);
            var innerMethodDelegateArgument = GetInnerMethodDelegateArgument(innerMethod, method);

            WriteProceedCall(method.DeclaringType, instructions, context, targetArgument, parametersArgument, methodArgument, innerMethodArgument, innerMethodDelegateArgument,
                typeArgument, abstractedArgument, genericParametersArgument);

            backCopy(instructions);

            // get return value
            if (method.HasReturnType)
                instructions.EmitUnboxOrCastIfNecessary(method.ReturnType);
            else
                instructions.Emit(OpCodes.Pop); // if no return type, ignore Proceed() result

            // loads back out/ref parameters
            var methodParameters = new MethodParameters(method);
            for (int parameterIndex = 0; parameterIndex < methodParameters.Count; parameterIndex++)
            {
                var parameter = methodParameters[parameterIndex];
                if (parameter.Type is ByRefSig)
                {
                    instructions.EmitLdarg(parameter); // loads given parameter (it is a ref)
                    instructions.EmitLdloc(parametersVariable); // array
                    instructions.EmitLdc(parameterIndex); // array index
                    instructions.Emit(OpCodes.Ldelem_Ref); // now we have boxed out/ref value
                    var parameterElementType = parameter.Type.Next;
                    instructions.EmitUnboxOrCastIfNecessary(parameterElementType);
                    instructions.EmitStind(parameterElementType); // result is stored in ref parameter
                }
            }

            // and return
            instructions.Emit(OpCodes.Ret);

            method.Body.PdbMethod = new PdbMethod { Scope = new PdbScope { Start = method.Body.Instructions[0] } };
            method.Body.PdbMethod.Scope.Scopes.Add(new PdbScope { Start = method.Body.Instructions[0] });
        }

        /// <summary>
        /// Writes the invocation call.
        /// </summary>
        /// <param name="methodDeclaringType"></param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="context">The context.</param>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private void WriteProceedCall(TypeDef methodDeclaringType, Instructions instructions, WeavingContext context, params InvocationArgument[] arguments)
        {
            var proceedMethod = GetProceedMethod(methodDeclaringType, arguments, instructions.Module, context);

            foreach (var argument in arguments)
            {
                if (argument.HasValue || methodDeclaringType.IsValueType)
                    argument.Emit(instructions);
            }

            instructions.Emit(OpCodes.Call, proceedMethod);
        }

        private IMethod GetProceedMethod(TypeDef methodDeclaringType, InvocationArgument[] arguments, ModuleDef module, WeavingContext context)
        {
            if (methodDeclaringType.IsValueType)
                return GetDefaultProceedMethod(module, context);

            var values = arguments.Select(a => a.HasValue).ToArray();
            if (!context.ShortcutMethods.TryGetValue(values, out var proceedMethod))
                context.ShortcutMethods[values] = proceedMethod = LoadProceedMethod(arguments, module, context);
            return proceedMethod;
        }

        private IMethod LoadProceedMethod(InvocationArgument[] arguments, ModuleDef module, WeavingContext context)
        {
            // special case, full invoke
            if (arguments.All(a => a.HasValue))
                return GetDefaultProceedMethod(module, context);

            return CreateProceedMethod(arguments, module, context);
        }

        private IMethod GetDefaultProceedMethod(ModuleDef module, WeavingContext context)
        {
            if (context.InvocationProceedMethod == null)
            {
                var invocationType = TypeResolver.Resolve(module, typeof(Invocation));
                if (invocationType == null)
                    throw new InvalidOperationException();
                var proceedMethodReference = invocationType.Methods.SingleOrDefault(m => m.IsStatic && m.Name == nameof(Invocation.ProceedAdvice2));
                if (proceedMethodReference == null)
                    throw new InvalidOperationException();
                context.InvocationProceedMethod = module.SafeImport(proceedMethodReference);
            }
            return context.InvocationProceedMethod;
        }

        /// <summary>
        /// Finds the type of the shortcut.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        public TypeDef FindShortcutType(ModuleDef module)
        {
            return module.Find($"{ShortcutTypeNamespace}.{ShortcutTypeName}", true);
        }

        private IMethod CreateProceedMethod(IReadOnlyList<InvocationArgument> arguments, ModuleDef module, WeavingContext context)
        {
            // get the class from shortcuts
            var shortcutType = context.ShortcutClass;
            if (shortcutType == null)
            {
                shortcutType = new TypeDefUser(ShortcutTypeNamespace, ShortcutTypeName)
                {
                    BaseType = module.Import(module.CorLibTypes.Object).ToTypeDefOrRef(),
                    // Abstract + Sealed is Static class
                    Attributes = TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed
                };
                module.Types.Add(shortcutType);
                context.ShortcutClass = shortcutType;
            }

            // create the method
            var nameBuilder = new StringBuilder("ProceedAspect");
            var argumentIndex = 0;
            var methodSig = new MethodSig { RetType = module.CorLibTypes.Object, HasThis = false };
            var defaultProceedMethod = GetDefaultProceedMethod(module, context);
            foreach (var argument in arguments)
            {
                if (argument.HasValue)
                    methodSig.Params.Add(defaultProceedMethod.MethodSig.Params[argumentIndex]);
                // One day if there are arguments collision risks (IE optional arguments with same type), overload name
                argumentIndex++;
            }
            var method = new MethodDefUser(nameBuilder.ToString(), methodSig)
            {
                Body = new CilBody(),
                Attributes = MethodAttributes.Public | MethodAttributes.Static
            };
            shortcutType.Methods.Add(method);
            var instructions = new Instructions(method.Body, module);

            // now, either get value from given arguments or from default
            argumentIndex = 0;
            var usedArgumentIndex = 0;
            foreach (var argument in arguments)
            {
                if (argument.HasValue) // a given argument
                    instructions.EmitLdarg(method.Parameters[usedArgumentIndex++]);
                else
                    arguments[argumentIndex].EmitDefault(instructions);
                argumentIndex++;
            }

            instructions.Emit(OpCodes.Tailcall); // because target method returns object and this method also returns an object
            instructions.Emit(OpCodes.Call, defaultProceedMethod);
            instructions.Emit(OpCodes.Ret);

            return method;
        }

        private InvocationArgument GetTargetArgument(MethodDef method, out Action<Instructions> backCopy)
        {
            var isStatic = method.IsStatic;
            var boxed = new Local(method.Module.CorLibTypes.Object, "boxed");
            TypeSig declaringTypeSig;
            var isGeneric = method.DeclaringType.HasGenericParameters;
            if (isGeneric)
            {
                var genericTypeArgs = new List<TypeSig>();
                for (int genericTypeArgIndex = 0; genericTypeArgIndex < method.DeclaringType.GenericParameters.Count; genericTypeArgIndex++)
                    genericTypeArgs.Add(new GenericVar(genericTypeArgIndex, method.DeclaringType));
                declaringTypeSig = new GenericInstSig((ClassOrValueTypeSig)method.DeclaringType.ToTypeSig(), genericTypeArgs);
            }
            else
                declaringTypeSig = method.DeclaringType.ToTypeSig();

            // for value types, the this is boxed to be advised (good luck managing this anyway)
            // so the boxed value content needs to be copied back to current instance
            var boxUnboxValue = method.DeclaringType.IsValueType;
            if (boxUnboxValue)
            {
                // this unboxes and copies back to this (generates a "this=(TValue)boxed")
                backCopy = delegate (Instructions i)
                {
                    i.Emit(OpCodes.Ldarg_0);
                    i.EmitLdloc(boxed);
                    i.Emit(OpCodes.Unbox_Any, declaringTypeSig);
                    i.Emit(OpCodes.Stobj, declaringTypeSig);
                };
            }
            else
                backCopy = delegate { };
            return new InvocationArgument("This", !isStatic, delegate (Instructions i)
            {
                i.Emit(OpCodes.Ldarg_0);
                // value type has to be boxed
                if (boxUnboxValue)
                {
                    i.Variables.Add(boxed);

                    i.Emit(OpCodes.Ldobj, declaringTypeSig);
                    i.Emit(OpCodes.Box, declaringTypeSig);

                    i.Emit(OpCodes.Dup);
                    i.EmitStloc(boxed);
                }
                // to fix peverify 0x80131854
                else if (method.IsConstructor)
                    i.Emit(OpCodes.Castclass, method.Module.CorLibTypes.Object);
            }, i => i.Emit(OpCodes.Ldnull));
        }

        private static InvocationArgument GetParametersArgument(MethodDef method, out Local parametersVariable)
        {
            var methodParameters = new MethodParameters(method);
            var hasParameters = methodParameters.Count > 0;
            var localParametersVariable = parametersVariable = hasParameters ? new Local(new SZArraySig(method.Module.CorLibTypes.Object)) { Name = "parameters" } : null;
            return new InvocationArgument("Parameters", hasParameters,
                delegate (Instructions instructions)
                {
                    method.Body.Variables.Add(localParametersVariable);

                    instructions.EmitLdc(methodParameters.Count);
                    instructions.Emit(OpCodes.Newarr, method.Module.CorLibTypes.Object);
                    instructions.EmitStloc(localParametersVariable);
                    // setups parameters array
                    for (int parameterIndex = 0; parameterIndex < methodParameters.Count; parameterIndex++)
                    {
                        var parameter = methodParameters[parameterIndex];
                        // we don't care about output parameters
                        if (!parameter.ParamDef.IsOut)
                        {
                            instructions.EmitLdloc(localParametersVariable); // array
                            instructions.EmitLdc(parameterIndex); // array index
                            instructions.EmitLdarg(parameter); // loads given parameter...
                            var parameterType = parameter.Type;
                            if (parameterType is ByRefSig) // ...if ref, loads it as referenced value
                            {
                                parameterType = parameter.Type.Next;
                                if (parameterType.IsGenericParameter)
                                    instructions.Emit(OpCodes.Ldobj, parameterType);
                                else
                                    instructions.EmitLdind(parameterType);
                            }
                            instructions.EmitBoxIfNecessary(parameterType); // ... and boxes it
                            instructions.Emit(OpCodes.Stelem_Ref);
                        }
                    }
                    instructions.EmitLdloc(localParametersVariable);
                }, instructions => instructions.Emit(OpCodes.Ldnull));
        }

        private static InvocationArgument GetMethodArgument(MethodDef method)
        {
            return new InvocationArgument("Method", true, instructions => instructions.Emit(OpCodes.Ldtoken, method), null);
        }

        private static InvocationArgument GetInnerMethodArgument(MethodDef innerMethod)
        {
            return new InvocationArgument("InnerMethod", innerMethod != null,
                instructions => instructions.Emit(OpCodes.Ldtoken, innerMethod),
                instructions => instructions.Emit(OpCodes.Dup));
        }

        private static InvocationArgument GetTypeArgument(MethodDef method)
        {
            return new InvocationArgument("Type", method.DeclaringType.HasGenericParameters,
                instructions => instructions.Emit(OpCodes.Ldtoken, method.DeclaringType),
                instructions => instructions.Emit(OpCodes.Ldtoken, method.Module.CorLibTypes.Void));
        }

        private static InvocationArgument GetAbstractedArgument(bool abstractedTarget)
        {
            return new InvocationArgument("Abstracted", abstractedTarget,
                i => i.Emit(OpCodes.Ldc_I4_1),
                i => i.Emit(OpCodes.Ldc_I4_0));
        }

        private static InvocationArgument GetGenericParametersArgument(MethodDef method)
        {
            // on static methods from generic type, we also record the generic parameters type
            //var typeGenericParametersCount = isStatic ? method.DeclaringType.GenericParameters.Count : 0;
            var typeGenericParametersCount = method.DeclaringType.GenericParameters.Count;
            var hasGeneric = typeGenericParametersCount > 0 || method.HasGenericParameters;
            // if method has generic parameters, we also pass them to Proceed method
            var genericParametersVariable = hasGeneric
                ? new Local(new SZArraySig(method.Module.SafeImport(typeof(Type)).ToTypeSig())) { Name = "genericParameters" }
                : null;
            return new InvocationArgument("GenericArguments", hasGeneric,
                delegate (Instructions instructions)
                {
                    //IL_0001: ldtoken !!T
                    //IL_0006: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
                    method.Body.Variables.Add(genericParametersVariable);

                    instructions.EmitLdc(typeGenericParametersCount + method.GenericParameters.Count);
                    instructions.Emit(OpCodes.Newarr, method.Module.SafeImport(typeof(Type)));
                    instructions.EmitStloc(genericParametersVariable);

                    var methodGenericParametersCount = method.GenericParameters.Count;
                    for (int genericParameterIndex = 0;
                        genericParameterIndex < typeGenericParametersCount + methodGenericParametersCount;
                        genericParameterIndex++)
                    {
                        instructions.EmitLdloc(genericParametersVariable); // array
                        instructions.EmitLdc(genericParameterIndex); // array index
                        if (genericParameterIndex < typeGenericParametersCount)
                            instructions.Emit(OpCodes.Ldtoken, new GenericVar(genericParameterIndex, method.DeclaringType));
                        //genericParameters[genericParameterIndex]);
                        else
                            instructions.Emit(OpCodes.Ldtoken, new GenericMVar(genericParameterIndex - typeGenericParametersCount, method));
                        //genericParameters[genericParameterIndex]);
                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        instructions.Emit(OpCodes.Call, ReflectionUtility.GetMethodInfo(() => Type.GetTypeFromHandle(new RuntimeTypeHandle())));
                        instructions.Emit(OpCodes.Stelem_Ref);
                    }
                    instructions.EmitLdloc(genericParametersVariable);
                }, instructions => instructions.Emit(OpCodes.Ldnull));
        }

        private InvocationArgument GetInnerMethodDelegateArgument(MethodDef innerMethod, MethodDef method)
        {
            var innerMethodDelegate = WriteDelegateProceeder(innerMethod, method.Name, new MethodParameters(method), method.Module);
            return new InvocationArgument("InnerMethodDelegate", innerMethodDelegate != null,
                instructions => instructions.Emit(OpCodes.Ldtoken, innerMethodDelegate),
                instructions => instructions.Emit(OpCodes.Dup));
        }

        /// <summary>
        /// Weaves the introductions.
        /// Introduces members as requested by aspects
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="context">The context.</param>
        private void WeaveIntroductions(MethodDef method, ModuleDef moduleDefinition, WeavingContext context)
        {
            var typeDefinition = method.DeclaringType;
            var advices = GetAllMarkers(new MethodReflectionNode(method, null), context.AdviceInterfaceType, context);
            var markerAttributeCtor = moduleDefinition.SafeImport(TypeResolver.Resolve(moduleDefinition, typeof(IntroducedFieldAttribute)).FindConstructors().Single());
            var markerAttributeCtorDef = new MemberRefUser(markerAttributeCtor.Module, markerAttributeCtor.Name, markerAttributeCtor.MethodSig, markerAttributeCtor.DeclaringType);
            foreach (var advice in advices)
            {
                var adviceDefinition = advice.Item2.Type;
                foreach (var field in adviceDefinition.Fields.Where(f => f.IsPublic))
                {
                    IntroduceMember(method.Module, field.Name, field.FieldType.ToTypeDefOrRef(), field.IsStatic, adviceDefinition, typeDefinition,
                        markerAttributeCtorDef, advice.Item1.Name, field.IsNotSerialized, context);
                }
                foreach (var property in adviceDefinition.Properties.Where(p => p.HasAnyPublic()))
                {
                    IntroduceMember(method.Module, property.Name, property.PropertySig.RetType.ToTypeDefOrRef(), !property.PropertySig.HasThis, adviceDefinition,
                        typeDefinition, markerAttributeCtorDef, advice.Item1.Name, false, context);
                }
            }
        }

        /// <summary>
        /// Weaves the information advices.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="infoAdviceInterface">The information advice interface.</param>
        /// <param name="context">The context.</param>
        private void WeaveInfoAdvices(ModuleDef moduleDefinition, TypeDef typeDefinition, ITypeDefOrRef infoAdviceInterface, WeavingContext context)
        {
            if (GetMarkedMethods(new TypeReflectionNode(typeDefinition, null), infoAdviceInterface, context).Where(IsWeavable).Any())
            {
                Logging.WriteDebug("Weaving type '{0}' for info", typeDefinition.FullName);
                WeaveInfoAdvices(typeDefinition, moduleDefinition, false);
            }
        }

        /// <summary>
        /// Weaves the method.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="markedMethod">The marked method.</param>
        /// <param name="context">The context.</param>
        private void WeaveMethod(ModuleDef moduleDefinition, MarkedNode markedMethod, WeavingContext context)
        {
            var method = markedMethod.Node.Method;
            try
            {
                WeaveAdvices(markedMethod, context);
                WeaveIntroductions(method, moduleDefinition, context);
            }
            catch (Exception e)
            {
                Logging.WriteError("Error while weaving method '{0}': {1}", method.FullName, e);
            }
        }

        /// <summary>
        /// Weaves the interface.
        /// What we do here is:
        /// - creating a class (which is named after the interface name)
        /// - this class implements all interface members
        /// - all members invoke Invocation.ProcessInterfaceMethod
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="context">The context.</param>
        private void WeaveInterface(ModuleDefMD moduleDefinition, TypeDef interfaceType, WeavingContext context)
        {
            var importedInterfaceType = moduleDefinition.Import(interfaceType);
            Logging.WriteDebug("Weaving interface '{0}'", interfaceType.FullName);
            TypeDef implementationType;
            TypeDef advisedInterfaceType;
            lock (moduleDefinition)
            {
                // ensure we're creating the interface only once
                var implementationTypeName = GetImplementationTypeName(interfaceType.Name);
                var implementationTypeNamespace = interfaceType.Namespace;
                if (moduleDefinition.GetTypes().Any(t => t.Namespace == implementationTypeNamespace && t.Name == implementationTypeName))
                    return;

                // now, create the implementation type
                var typeAttributes = (InjectAsPrivate ? TypeAttributes.NotPublic : TypeAttributes.Public) | TypeAttributes.Class |
                                     TypeAttributes.BeforeFieldInit;
                advisedInterfaceType = TypeResolver.Resolve(moduleDefinition, typeof(AdvisedInterface));
                // TODO: this should work using TypeImporter.Import
                var advisedInterfaceTypeReference = moduleDefinition.Import(advisedInterfaceType);
                implementationType = new TypeDefUser(implementationTypeNamespace, implementationTypeName, advisedInterfaceTypeReference) { Attributes = typeAttributes };
                implementationType.Interfaces.Add(new InterfaceImplUser(importedInterfaceType));

                lock (moduleDefinition)
                    moduleDefinition.Types.Add(implementationType);
            }

            // create empty .ctor. This .NET mofo wants it!
            var baseEmptyConstructor = moduleDefinition.SafeImport(advisedInterfaceType.FindConstructors().Single());
            const MethodAttributes ctorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                                                    MethodAttributes.RTSpecialName;
            var method = new MethodDefUser(".ctor", baseEmptyConstructor.MethodSig, ctorAttributes);
            method.Body = new CilBody();
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, baseEmptyConstructor));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            implementationType.Methods.Add(method);

            // create implementation methods
            foreach (var currentInterfaceType in interfaceType.GetAllInterfaces(TypeResolver))
            {
                foreach (var interfaceMethod in currentInterfaceType.Methods.Where(m => !m.IsSpecialName))
                    WeaveInterfaceMethod(interfaceMethod, implementationType, true, context);

                // create implementation properties
                foreach (var interfaceProperty in currentInterfaceType.Properties)
                {
                    var implementationProperty = new PropertyDefUser(interfaceProperty.Name, interfaceProperty.PropertySig);
                    implementationType.Properties.Add(implementationProperty);
                    if (interfaceProperty.GetMethod != null)
                        implementationProperty.GetMethod = WeaveInterfaceMethod(interfaceProperty.GetMethod, implementationType, InjectAsPrivate, context);
                    if (interfaceProperty.SetMethod != null)
                        implementationProperty.SetMethod = WeaveInterfaceMethod(interfaceProperty.SetMethod, implementationType, InjectAsPrivate, context);
                }

                // create implementation events
                foreach (var interfaceEvent in currentInterfaceType.Events)
                {
                    var implementationEvent = new EventDefUser(interfaceEvent.Name, interfaceEvent.EventType);
                    implementationType.Events.Add(implementationEvent);
                    if (interfaceEvent.AddMethod != null)
                        implementationEvent.AddMethod = WeaveInterfaceMethod(interfaceEvent.AddMethod, implementationType, InjectAsPrivate, context);
                    if (interfaceEvent.RemoveMethod != null)
                        implementationEvent.RemoveMethod = WeaveInterfaceMethod(interfaceEvent.RemoveMethod, implementationType, InjectAsPrivate, context);
                }
            }
        }

        /// <summary>
        /// Creates the advice wrapper and adds it to implementation.
        /// </summary>
        /// <param name="interfaceMethod">The interface method.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="injectAsPrivate">if set to <c>true</c> [inject as private].</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private MethodDef WeaveInterfaceMethod(MethodDef interfaceMethod, TypeDef implementationType, bool injectAsPrivate, WeavingContext context)
        {
            var module = implementationType.Module;
            var typeImporter = new TypeImporter(module);
            var methodAttributes = MethodAttributes.NewSlot | MethodAttributes.Virtual | (injectAsPrivate ? MethodAttributes.Public : MethodAttributes.Private);
            var implementationMethodSig = interfaceMethod.MethodSig.Clone();
            var implementationMethod = new MethodDefUser(interfaceMethod.Name, /*interfaceMethod.MethodSig */implementationMethodSig, methodAttributes);
            for (int parameterIndex = 0; parameterIndex < implementationMethod.Parameters.Count; parameterIndex++)
            {
                var parameterType = implementationMethod.Parameters[parameterIndex].Type;
                if (parameterType != null)
                {
                    var relocatedParameterType = typeImporter.TryRelocateTypeSig(parameterType) ?? parameterType;
                    implementationMethod.Parameters[parameterIndex].Type = module.Import(relocatedParameterType);
                }
            }
            if (implementationMethod.HasReturnType)
            {
                var relocatedReturnType = typeImporter.TryRelocateTypeSig(implementationMethod.ReturnType) ?? implementationMethod.ReturnType;
                implementationMethod.ReturnType = relocatedReturnType;
            }
            implementationMethod.GenericParameters.AddRange(interfaceMethod.GenericParameters);
            implementationType.Methods.Add(implementationMethod);
            var interfaceMethodRef = module.Import(interfaceMethod);
            implementationMethod.Overrides.Add(new MethodOverride(implementationMethod, interfaceMethodRef));
            WritePointcutBody(implementationMethod, null, false, context);
            return implementationMethod;
        }

        /// <summary>
        /// Introduces the member.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="memberType">Type of the member.</param>
        /// <param name="isStatic">if set to <c>true</c> [is static].</param>
        /// <param name="adviceType">The advice.</param>
        /// <param name="advisedType">The type definition.</param>
        /// <param name="markerAttribute">The marker attribute ctor.</param>
        /// <param name="introducedMemberName">Name of the introduced member.</param>
        /// <param name="isNotSerialized">if set to <c>true</c> [is not serialized].</param>
        /// <param name="context">The context.</param>
        private void IntroduceMember(ModuleDef moduleDefinition, string memberName, ITypeDefOrRef memberType, bool isStatic,
            ITypeDefOrRef adviceType, TypeDef advisedType, ICustomAttributeType markerAttribute, string introducedMemberName, bool isNotSerialized, WeavingContext context)
        {
            if (IsIntroduction(memberType, out var introducedFieldType, out var isShared, context))
            {
                var introducedFieldName = IntroductionRules.GetName(adviceType.Namespace, adviceType.Name, isShared ? null : introducedMemberName, memberName);
                IntroduceMember(moduleDefinition, advisedType, markerAttribute, isStatic, isNotSerialized, introducedFieldName, introducedFieldType.ToTypeSig());
                // also introduce registry (this is done once anyway)
                IntroduceMember(moduleDefinition, advisedType, markerAttribute, false, true, IntroductionRules.RegistryName, context.IntroducedFieldsType.ToTypeSig()); // introduced as object by pure laziness
            }
        }

        private void IntroduceMember(ModuleDef moduleDefinition, TypeDef advisedType, ICustomAttributeType markerAttribute, bool isStatic, bool isNotSerialized,
            string introducedFieldName,
            TypeSig introducedFieldType)
        {
            lock (advisedType.Fields)
            {
                if (advisedType.Fields.All(f => f.Name != introducedFieldName))
                {
                    var fieldAttributes = (InjectAsPrivate ? FieldAttributes.Private : FieldAttributes.Public)
                                          | (isNotSerialized ? FieldAttributes.NotSerialized : 0);
                    if (isStatic)
                        fieldAttributes |= FieldAttributes.Static;
                    Logging.WriteDebug("Introduced field type '{0}'", introducedFieldType.FullName);
                    var introducedFieldTypeReference = TypeImporter.Import(moduleDefinition, introducedFieldType);
                    var introducedField = new FieldDefUser(introducedFieldName, new FieldSig(introducedFieldTypeReference), fieldAttributes);
                    introducedField.CustomAttributes.Add(new CustomAttribute(markerAttribute));
                    advisedType.Fields.Add(introducedField);
                }
            }
        }
    }
}