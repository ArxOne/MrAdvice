#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Annotation;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using dnlib.DotNet.Pdb;
    using Introduction;
    using IO;
    using Reflection;
    using Reflection.Groups;
    using Utility;
    using FieldAttributes = dnlib.DotNet.FieldAttributes;
    using MethodAttributes = dnlib.DotNet.MethodAttributes;
    using TypeAttributes = dnlib.DotNet.TypeAttributes;

    partial class AspectWeaver
    {
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

            // the cctor needs to be called after all initialization (in case some info advices collect data)
            infoAdvisedType.Attributes &= ~TypeAttributes.BeforeFieldInit;

            const string cctorMethodName = ".cctor";
            var staticCtor = infoAdvisedType.Methods.SingleOrDefault(m => m.Name == cctorMethodName);
            var newStaticCtor = staticCtor == null;
            if (newStaticCtor)
            {
                staticCtor = new MethodDefUser(cctorMethodName, MethodSig.CreateStatic(moduleDefinition.CorLibTypes.Void),
                    (InjectAsPrivate ? MethodAttributes.Private : MethodAttributes.Public)
                    | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
                staticCtor.Body = new CilBody();
                infoAdvisedType.Methods.Add(staticCtor);
            }

            var instructions = new Instructions(staticCtor.Body.Instructions, staticCtor.Module);

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
            // ret is only emitted if the method is new
            if (newStaticCtor)
                instructions.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Weaves the specified method.
        /// </summary>
        /// <param name="markedMethod">The marked method.</param>
        /// <param name="types">The types.</param>
        private void WeaveAdvices(MarkedNode markedMethod, Types types)
        {
            var method = markedMethod.Node.Method;

            // sanity check
            var moduleDefinition = (ModuleDefMD)method.Module;
            if (method.ReturnType.SafeEquivalent(moduleDefinition.SafeImport(typeof(void)).ToTypeSig()))
            {
                var customAttributes = method.CustomAttributes;
                if (customAttributes.Any(c => c.AttributeType.Name == "AsyncStateMachineAttribute"))
                    Logging.WriteWarning("Advising async void method '{0}' could confuse async advices. Consider switching its return type to async Task.", method.FullName);
            }

            if (method.IsAbstract)
            {
                method.Attributes = (method.Attributes & ~MethodAttributes.Abstract) | MethodAttributes.Virtual;
                Logging.WriteDebug("Weaving abstract method '{0}'", method.FullName);
                WritePointcutBody(method, null, false);
            }
            else if (markedMethod.AbstractTarget)
            {
                Logging.WriteDebug("Weaving and abstracting method '{0}'", method.FullName);
                WritePointcutBody(method, null, true);
            }
            else
            {
                Logging.WriteDebug("Weaving method '{0}'", method.FullName);

                var methodName = method.Name;

                // our special recipe, with weaving advices
                var weavingAdvicesMarkers = GetAllMarkers(markedMethod.Node, types.WeavingAdviceAttributeType, types).ToArray();
                if (weavingAdvicesMarkers.Any())
                {
                    var typeDefinition = markedMethod.Node.Method.DeclaringType;
                    var initialType = TypeLoader.GetType(typeDefinition);
                    var weaverMethodWeavingContext = new WeaverMethodWeavingContext(typeDefinition, initialType, methodName, types, TypeResolver, Logging);
                    foreach (var weavingAdviceMarker in weavingAdvicesMarkers)
                    {
                        var weavingAdviceType = TypeLoader.GetType(weavingAdviceMarker.Type);
                        var weavingAdvice = (IWeavingAdvice)Activator.CreateInstance(weavingAdviceType);
                        var methodWeavingAdvice = weavingAdvice as IMethodWeavingAdvice;
                        if (methodWeavingAdvice != null && !method.IsGetter && !method.IsSetter)
                        {
                            methodWeavingAdvice.Advise(weaverMethodWeavingContext);
                        }
                    }
                    if (weaverMethodWeavingContext.TargetMethodName != methodName)
                        methodName = method.Name = weaverMethodWeavingContext.TargetMethodName;
                }

                // create inner method
                const MethodAttributes attributesToKeep = MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.PInvokeImpl |
                                                          MethodAttributes.UnmanagedExport | MethodAttributes.HasSecurity |
                                                          MethodAttributes.RequireSecObject;
                var innerMethodAttributes = method.Attributes & attributesToKeep |
                                            (InjectAsPrivate ? MethodAttributes.Private : MethodAttributes.Public);
                string innerMethodName;
                if (method.IsGetter)
                    innerMethodName = GetPropertyInnerGetterName(GetPropertyName(methodName));
                else if (method.IsSetter)
                    innerMethodName = GetPropertyInnerSetterName(GetPropertyName(methodName));
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

                WritePointcutBody(method, innerMethod, false);
                lock (method.DeclaringType)
                    method.DeclaringType.Methods.Add(innerMethod);
            }
        }

        /// <summary>
        /// Writes the pointcut body.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="innerMethod">The inner method.</param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        private void WritePointcutBody(MethodDef method, MethodDef innerMethod, bool abstractedTarget)
        {
            var moduleDefinition = method.Module;

            // now empty the old one and make it call the inner method...
            if (method.Body == null)
                method.Body = new CilBody();
            method.Body.InitLocals = true;
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
            method.Body.ExceptionHandlers.Clear();
            var instructions = new Instructions(method.Body.Instructions, method.Module);

            var targetParameter = GetTargetArgument(method);
            Local parametersVariable;
            var parametersParameters = GetParametersArgument(method, out parametersVariable);
            var methodParameter = GetMethodArgument(method);
            var innerMethodParameter = GetInnerMethodArgument(innerMethod);
            var typeParameter = GetTypeArgument(method);
            var abstractedParameter = GetAbstractedArgument(abstractedTarget);

            var genericParametersParameter = GetGenericParametersArgument(method);

            targetParameter.Emit(instructions);
            parametersParameters.Emit(instructions);
            methodParameter.Emit(instructions);
            innerMethodParameter.Emit(instructions);
            typeParameter.Emit(instructions);
            abstractedParameter.Emit(instructions);
            genericParametersParameter.Emit(instructions);

            // invoke the method
            var invocationType = TypeResolver.Resolve(moduleDefinition, typeof(Invocation));
            if (invocationType == null)
                throw new InvalidOperationException();
            var proceedMethodReference = invocationType.Methods.SingleOrDefault(m => m.IsStatic && m.Name == nameof(Invocation.ProceedAdvice));
            if (proceedMethodReference == null)
                throw new InvalidOperationException();
            var proceedMethod = moduleDefinition.SafeImport(proceedMethodReference);

            instructions.Emit(OpCodes.Call, proceedMethod);

            // get return value
            if (!method.ReturnType.SafeEquivalent(moduleDefinition.CorLibTypes.Void))
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
                    // TODO reimplement
                    if (parameterElementType.IsGenericInstanceType)
                    {
                        //var z = (GenericInstSig) parameterElementType;
                        //parameterElementType = z.GenericType;
                    }
                    //if (parameterElementType.IsGenericInstanceType) // a generic type requires the correct inner type
                    //{
                    //    var referenceParameterType = (ByReferenceType)parameter.ParameterType;
                    //    parameterElementType = (GenericInstanceType)referenceParameterType.ElementType;
                    //}
                    instructions.EmitUnboxOrCastIfNecessary(parameterElementType);
                    instructions.EmitStind(parameterElementType); // result is stored in ref parameter
                }
            }

            // and return
            instructions.Emit(OpCodes.Ret);

            method.Body.Scope = new PdbScope { Start = method.Body.Instructions[0] };
            method.Body.Scope.Scopes.Add(new PdbScope { Start = method.Body.Instructions[0] });
        }

        private InvocationArgument GetTargetArgument(MethodDef method)
        {
            var isStatic = method.IsStatic;
            return new InvocationArgument(!isStatic, delegate (Instructions i)
            {
                i.Emit(OpCodes.Ldarg_0);
                // to fix peverify 0x80131854
                if (method.IsConstructor)
                    i.Emit(OpCodes.Castclass, method.Module.CorLibTypes.Object);
            }, i => i.Emit(OpCodes.Ldnull));
        }

        private InvocationArgument GetParametersArgument(MethodDef method, out Local parametersVariable)
        {
            var methodParameters = new MethodParameters(method);
            var hasParameters = methodParameters.Count > 0;
            var localParametersVariable = parametersVariable = hasParameters ? new Local(new SZArraySig(method.Module.CorLibTypes.Object)) { Name = "parameters" } : null;
            return new InvocationArgument(hasParameters,
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
                                instructions.EmitLdind(parameterType);
                            }
                            instructions.EmitBoxIfNecessary(parameterType); // ... and boxes it
                            instructions.Emit(OpCodes.Stelem_Ref);
                        }
                    }
                    instructions.EmitLdloc(localParametersVariable);
                }, instructions => instructions.Emit(OpCodes.Ldnull));
        }

        private InvocationArgument GetMethodArgument(MethodDef method)
        {
            return new InvocationArgument(true, instructions => instructions.Emit(OpCodes.Ldtoken, method), null);
        }

        private InvocationArgument GetInnerMethodArgument(MethodDef innerMethod)
        {
            return new InvocationArgument(innerMethod != null,
                instructions => instructions.Emit(OpCodes.Ldtoken, innerMethod),
                instructions => instructions.Emit(OpCodes.Dup));
        }

        private InvocationArgument GetTypeArgument(MethodDef method)
        {
            return new InvocationArgument(method.DeclaringType.HasGenericParameters,
                instructions => instructions.Emit(OpCodes.Ldtoken, method.DeclaringType),
                instructions => instructions.Emit(OpCodes.Ldtoken, method.Module.CorLibTypes.Void));
        }

        private InvocationArgument GetAbstractedArgument(bool abstractedTarget)
        {
            return new InvocationArgument(abstractedTarget,
                i => i.Emit(OpCodes.Ldc_I4_1),
                i => i.Emit(OpCodes.Ldc_I4_0));
        }

        private InvocationArgument GetGenericParametersArgument(MethodDef method)
        {
            // on static methods from generic type, we also record the generic parameters type
            //var typeGenericParametersCount = isStatic ? method.DeclaringType.GenericParameters.Count : 0;
            var typeGenericParametersCount = method.DeclaringType.GenericParameters.Count;
            var hasGeneric = typeGenericParametersCount > 0 || method.HasGenericParameters;
            // if method has generic parameters, we also pass them to Proceed method
            var genericParametersVariable = hasGeneric ? new Local(new SZArraySig(method.Module.SafeImport(typeof(Type)).ToTypeSig())) { Name = "genericParameters" } : null;
            return new InvocationArgument(hasGeneric,
                delegate (Instructions instructions)
                {
                    //IL_0001: ldtoken !!T
                    //IL_0006: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
                    method.Body.Variables.Add(genericParametersVariable);

                    instructions.EmitLdc(typeGenericParametersCount + method.GenericParameters.Count);
                    instructions.Emit(OpCodes.Newarr, method.Module.SafeImport(typeof(Type)));
                    instructions.EmitStloc(genericParametersVariable);

                    var methodGenericParametersCount = method.GenericParameters.Count;
                    for (int genericParameterIndex = 0; genericParameterIndex < typeGenericParametersCount + methodGenericParametersCount; genericParameterIndex++)
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

        /// <summary>
        /// Weaves the introductions.
        /// Introduces members as requested by aspects
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="adviceInterface">The advice interface.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="types">The types.</param>
        private void WeaveIntroductions(MethodDef method, TypeDef adviceInterface, ModuleDef moduleDefinition, Types types)
        {
            var typeDefinition = method.DeclaringType;
            var advices = GetAllMarkers(new MethodReflectionNode(method), adviceInterface, types);
            var markerAttributeCtor = moduleDefinition.SafeImport(TypeResolver.Resolve(moduleDefinition, typeof(IntroducedFieldAttribute)).FindConstructors().Single());
            var markerAttributeCtorDef = new MemberRefUser(markerAttributeCtor.Module, markerAttributeCtor.Name, markerAttributeCtor.MethodSig, markerAttributeCtor.DeclaringType);
            // moduleDefinition.SafeImport(markerAttributeCtor).ResolveMethodDef();
            foreach (var advice in advices)
            {
                var adviceDefinition = TypeResolver.Resolve(advice.Type);
                foreach (var field in adviceDefinition.Fields.Where(f => f.IsPublic))
                    IntroduceMember(method.Module, field.Name, field.FieldType.ToTypeDefOrRef(), field.IsStatic, advice.Type, typeDefinition, markerAttributeCtorDef);
                foreach (var property in adviceDefinition.Properties.Where(p => p.HasAnyPublic()))
                    IntroduceMember(method.Module, property.Name, property.PropertySig.RetType.ToTypeDefOrRef(), !property.PropertySig.HasThis, advice.Type, typeDefinition, markerAttributeCtorDef);
            }
        }

        /// <summary>
        /// Weaves the information advices.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="infoAdviceInterface">The information advice interface.</param>
        /// <param name="types">The types.</param>
        private void WeaveInfoAdvices(ModuleDef moduleDefinition, TypeDef typeDefinition, ITypeDefOrRef infoAdviceInterface, Types types)
        {
            if (GetMarkedMethods(new TypeReflectionNode(typeDefinition), infoAdviceInterface, types).Where(IsWeavable).Any())
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
        /// <param name="adviceInterface">The advice interface.</param>
        /// <param name="types">The types.</param>
        private void WeaveMethod(ModuleDef moduleDefinition, MarkedNode markedMethod, TypeDef adviceInterface, Types types)
        {
            var method = markedMethod.Node.Method;
            try
            {
                WeaveAdvices(markedMethod, types);
                WeaveIntroductions(method, adviceInterface, moduleDefinition, types);
            }
            catch (Exception e)
            {
                Logging.WriteError("Error while weaving method '{0}': {1}", method.FullName, e);
            }
        }

        /// <summary>
        /// Weaves the interface.
        /// What we do here is:
        /// - creating a class (wich is named after the interface name)
        /// - this class implements all interface members
        /// - all members invoke Invocation.ProcessInterfaceMethod
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="interfaceType">Type of the interface.</param>
        private void WeaveInterface(ModuleDef moduleDefinition, ITypeDefOrRef interfaceType)
        {
            Logging.WriteDebug("Weaving interface '{0}'", interfaceType.FullName);
            TypeDef implementationType;
            TypeDef advisedInterfaceType;
            TypeDef interfaceTypeDefinition;
            lock (moduleDefinition)
            {
                // ensure we're creating the interface only once
                var implementationTypeName = GetImplementationTypeName(interfaceType.Name);
                var implementationTypeNamespace = interfaceType.Namespace;
                if (moduleDefinition.GetTypes().Any(t => t.Namespace == implementationTypeNamespace && t.Name == implementationTypeName))
                    return;

                // now, create the implementation type
                interfaceTypeDefinition = TypeResolver.Resolve(interfaceType);
                var typeAttributes = (InjectAsPrivate ? TypeAttributes.NotPublic : TypeAttributes.Public) | TypeAttributes.Class | TypeAttributes.BeforeFieldInit;
                advisedInterfaceType = TypeResolver.Resolve(moduleDefinition, typeof(AdvisedInterface));
                // TODO: this should work using TypeImporter.Import
                var advisedInterfaceTypeReference = moduleDefinition.Import(advisedInterfaceType);
                implementationType = new TypeDefUser(implementationTypeNamespace, implementationTypeName, advisedInterfaceTypeReference) { Attributes = typeAttributes };
                implementationType.Interfaces.Add(new InterfaceImplUser(interfaceTypeDefinition));

                lock (moduleDefinition)
                    moduleDefinition.Types.Add(implementationType);
            }

            // create empty .ctor. This .NET mofo wants it!
            var baseEmptyConstructor = moduleDefinition.SafeImport(TypeResolver.Resolve(advisedInterfaceType).FindConstructors().Single());
            const MethodAttributes ctorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var method = new MethodDefUser(".ctor", baseEmptyConstructor.MethodSig, ctorAttributes);
            method.Body = new CilBody();
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, baseEmptyConstructor));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            implementationType.Methods.Add(method);

            // create implementation methods
            foreach (var interfaceMethod in interfaceTypeDefinition.Methods.Where(m => !m.IsSpecialName))
                WeaveInterfaceMethod(interfaceMethod, implementationType, true);

            // create implementation properties
            foreach (var interfaceProperty in interfaceTypeDefinition.Properties)
            {
                var implementationProperty = new PropertyDefUser(interfaceProperty.Name, interfaceProperty.PropertySig);
                implementationType.Properties.Add(implementationProperty);
                if (interfaceProperty.GetMethod != null)
                    implementationProperty.GetMethod = WeaveInterfaceMethod(interfaceProperty.GetMethod, implementationType, InjectAsPrivate);
                if (interfaceProperty.SetMethod != null)
                    implementationProperty.SetMethod = WeaveInterfaceMethod(interfaceProperty.SetMethod, implementationType, InjectAsPrivate);
            }

            // create implementation events
            foreach (var interfaceEvent in interfaceTypeDefinition.Events)
            {
                var implementationEvent = new EventDefUser(interfaceEvent.Name, interfaceEvent.EventType);
                implementationType.Events.Add(implementationEvent);
                if (interfaceEvent.AddMethod != null)
                    implementationEvent.AddMethod = WeaveInterfaceMethod(interfaceEvent.AddMethod, implementationType, InjectAsPrivate);
                if (interfaceEvent.RemoveMethod != null)
                    implementationEvent.RemoveMethod = WeaveInterfaceMethod(interfaceEvent.RemoveMethod, implementationType, InjectAsPrivate);
            }
        }

        /// <summary>
        /// Creates the advice wrapper and adds it to implementation.
        /// </summary>
        /// <param name="interfaceMethod">The interface method.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="injectAsPrivate">if set to <c>true</c> [inject as private].</param>
        /// <returns></returns>
        private MethodDef WeaveInterfaceMethod(MethodDef interfaceMethod, TypeDef implementationType, bool injectAsPrivate)
        {
            var methodAttributes = MethodAttributes.NewSlot | MethodAttributes.Virtual | (injectAsPrivate ? MethodAttributes.Public : MethodAttributes.Private);
            //var methodParameters = new MethodParameters(interfaceMethod);
            //var implementationMethodSig = new MethodSig(interfaceMethod.CallingConvention, (uint)interfaceMethod.GenericParameters.Count, interfaceMethod.ReturnType,
            //    methodParameters.Select(p => p.Type).ToArray())
            //{
            //    HasThis = interfaceMethod.HasThis,
            //    ExplicitThis = interfaceMethod.ExplicitThis,
            //    CallingConvention = interfaceMethod.CallingConvention,
            //};
            var implementationMethod = new MethodDefUser(interfaceMethod.Name, interfaceMethod.MethodSig /*implementationMethodSig*/, methodAttributes);
            //implementationMethod.ReturnType = interfaceMethod.ReturnType;
            implementationType.Methods.Add(implementationMethod);
            //implementationMethod.IsSpecialName = interfaceMethod.IsSpecialName;
            //methodParameters.SetParamDefs(implementationMethod);
            implementationMethod.GenericParameters.AddRange(interfaceMethod.GenericParameters);
            implementationMethod.Overrides.Add(new MethodOverride(implementationMethod, interfaceMethod));
            WritePointcutBody(implementationMethod, null, false);
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
        private void IntroduceMember(ModuleDef moduleDefinition, string memberName, ITypeDefOrRef memberType, bool isStatic,
            ITypeDefOrRef adviceType, TypeDef advisedType, ICustomAttributeType markerAttribute)
        {
            ITypeDefOrRef introducedFieldType;
            if (IsIntroduction(memberType, out introducedFieldType))
            {
                var introducedFieldName = IntroductionRules.GetName(adviceType.Namespace, adviceType.Name, memberName);
                lock (advisedType.Fields)
                {
                    if (advisedType.Fields.All(f => f.Name != introducedFieldName))
                    {
                        var fieldAttributes = (InjectAsPrivate ? FieldAttributes.Private : FieldAttributes.Public) | FieldAttributes.NotSerialized;
                        if (isStatic)
                            fieldAttributes |= FieldAttributes.Static;
                        Logging.WriteDebug("Introduced field type '{0}'", introducedFieldType.FullName);
                        var introducedFieldTypeReference = TypeImporter.Import(moduleDefinition, introducedFieldType.ToTypeSig());
                        var introducedField = new FieldDefUser(introducedFieldName, new FieldSig(introducedFieldTypeReference), fieldAttributes);
                        introducedField.CustomAttributes.Add(new CustomAttribute(markerAttribute));
                        advisedType.Fields.Add(introducedField);
                    }
                }
            }
        }
    }
}
