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
    using System.Diagnostics.SymbolStore;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Annotation;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using dnlib.DotNet.Pdb;
    using Introduction;
    using IO;
    using Reflection.Groups;
    using Utility;
    using EventAttributes = dnlib.DotNet.EventAttributes;
    using FieldAttributes = dnlib.DotNet.FieldAttributes;
    using MethodAttributes = dnlib.DotNet.MethodAttributes;
    using PropertyAttributes = dnlib.DotNet.PropertyAttributes;
    using SymbolReaderCreator = dnlib.DotNet.Pdb.Managed.SymbolReaderCreator;
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
                Logger.WriteWarning("Info advice method not found");
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
                instructions.Emit(OpCodes.Ldtoken, moduleDefinition.SafeImport(infoAdvisedType));
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
                    Logger.WriteWarning("Advising async void method '{0}' could confuse async advices. Consider switching its return type to async Task.", method.FullName);
            }

            if (method.IsAbstract)
            {
                method.Attributes = (method.Attributes & ~MethodAttributes.Abstract) | MethodAttributes.Virtual;
                Logger.WriteDebug("Weaving abstract method '{0}'", method.FullName);
                WritePointcutBody(method, null, false);
            }
            else if (markedMethod.AbstractTarget)
            {
                Logger.WriteDebug("Weaving and abstracting method '{0}'", method.FullName);
                WritePointcutBody(method, null, true);
            }
            else
            {
                Logger.WriteDebug("Weaving method '{0}'", method.FullName);

                var methodName = method.Name;

                // our special recipe, with weaving advices
                var weavingAdvicesMarkers = GetAllMarkers(markedMethod.Node, types.WeavingAdviceAttributeType, types).ToArray();
                if (weavingAdvicesMarkers.Any())
                {
                    var typeDefinition = markedMethod.Node.Method.DeclaringType;
                    var initialType = TypeLoader.GetType(typeDefinition);
                    var weaverMethodWeavingContext = new WeaverMethodWeavingContext(typeDefinition, initialType, methodName, types, TypeResolver);
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
                //innerMethod.Parameters.AddRange(method.Parameters);
                if (method.IsPinvokeImpl)
                {
                    innerMethod.ImplMap = method.ImplMap;
                    method.ImplMap = null;
                    method.IsPreserveSig = false;
                    method.IsPinvokeImpl = false;
                }
                else
                {
                    //innerMethod.Body = new CilBody();
                    //innerMethod.Body.InitLocals = method.Body.InitLocals;
                    //innerMethod.Body.Instructions.AddRange(method.Body.Instructions);
                    //innerMethod.Body.Variables.AddRange(method.Body.Variables);
                    //innerMethod.Body.ExceptionHandlers.AddRange(method.Body.ExceptionHandlers);

                    //innerMethod.Body.Scope = method.Body.Scope;
                    //method.Body.Scope = null;
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

            var isStatic = method.IsStatic;

            // parameters
            Local parametersVariable = null;
            var methodParameters = new MethodParameters(method);
            if (methodParameters.Count > 0)
            {
                parametersVariable = new Local(new SZArraySig(moduleDefinition.CorLibTypes.Object)) { Name = "parameters" };
                method.Body.Variables.Add(parametersVariable);

                instructions.EmitLdc(methodParameters.Count);
                instructions.Emit(OpCodes.Newarr, moduleDefinition.CorLibTypes.Object);
                instructions.EmitStloc(parametersVariable);
                // setups parameters array
                for (int parameterIndex = 0; parameterIndex < methodParameters.Count; parameterIndex++)
                {
                    var parameter = methodParameters[parameterIndex];
                    // we don't care about output parameters
                    if (!parameter.ParamDef.IsOut)
                    {
                        instructions.EmitLdloc(parametersVariable); // array
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
            }

            // if method has generic parameters, we also pass them to Proceed method
            Local genericParametersVariable = null;
            // on static methods from generic type, we also record the generic parameters type
            //var typeGenericParametersCount = isStatic ? method.DeclaringType.GenericParameters.Count : 0;
            var typeGenericParametersCount = method.DeclaringType.GenericParameters.Count;
            if (typeGenericParametersCount > 0 || method.HasGenericParameters)
            {
                //IL_0001: ldtoken !!T
                //IL_0006: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
                genericParametersVariable = new Local(new SZArraySig(moduleDefinition.SafeImport(typeof(Type)).ToTypeSig())) { Name = "genericParameters" };
                method.Body.Variables.Add(genericParametersVariable);

                instructions.EmitLdc(typeGenericParametersCount + method.GenericParameters.Count);
                instructions.Emit(OpCodes.Newarr, moduleDefinition.SafeImport(typeof(Type)));
                instructions.EmitStloc(genericParametersVariable);

                var methodGenericParametersCount = method.GenericParameters.Count;
                for (int genericParameterIndex = 0; genericParameterIndex < typeGenericParametersCount + methodGenericParametersCount; genericParameterIndex++)
                {
                    instructions.EmitLdloc(genericParametersVariable); // array
                    instructions.EmitLdc(genericParameterIndex); // array index
                    if (genericParameterIndex < typeGenericParametersCount)
                        instructions.Emit(OpCodes.Ldtoken, new GenericVar(genericParameterIndex, method.DeclaringType)); //genericParameters[genericParameterIndex]);
                    else
                        instructions.Emit(OpCodes.Ldtoken, new GenericMVar(genericParameterIndex - typeGenericParametersCount, method)); //genericParameters[genericParameterIndex]);
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    instructions.Emit(OpCodes.Call, ReflectionUtility.GetMethodInfo(() => Type.GetTypeFromHandle(new RuntimeTypeHandle())));
                    instructions.Emit(OpCodes.Stelem_Ref);
                }
            }

            // null or instance
            instructions.Emit(isStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);
            // to fix peverify 0x80131854
            if (!isStatic && method.IsConstructor)
                instructions.Emit(OpCodes.Castclass, moduleDefinition.CorLibTypes.Object);

            // parameters
            if (parametersVariable != null)
                instructions.EmitLdloc(parametersVariable);
            else
                instructions.Emit(OpCodes.Ldnull);

            // methods...
            // ... target
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            instructions.Emit(OpCodes.Call, ReflectionUtility.GetMethodInfo(() => MethodBase.GetCurrentMethod()));

            // ... inner... If provided
            if (innerMethod != null)
            {
                // if type is generic, this is a bit more complex, because we need to pass the type
                if (method.DeclaringType.HasGenericParameters)
                {
                    // we want to reuse the MethodBase.GetCurrentMethod() result
                    // so it is stored into a variable, whose property DeclaringType is invoked later
                    var currentMethodVariable = new Local(moduleDefinition.SafeImport(typeof(MethodBase)).ToTypeSig()) { Name = "currentMethod" };
                    method.Body.Variables.Add(currentMethodVariable);
                    instructions.EmitStloc(currentMethodVariable);
                    instructions.EmitLdloc(currentMethodVariable);

                    instructions.Emit(OpCodes.Ldtoken, innerMethod);
                    instructions.EmitLdloc(currentMethodVariable);
                    instructions.Emit(OpCodes.Callvirt, ReflectionUtility.GetMethodInfo((Type t) => t.DeclaringType));
                    instructions.Emit(OpCodes.Callvirt, ReflectionUtility.GetMethodInfo((Type t) => t.TypeHandle));
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    instructions.Emit(OpCodes.Call, ReflectionUtility.GetMethodInfo(() => MethodBase.GetMethodFromHandle(new RuntimeMethodHandle(), new RuntimeTypeHandle())));
                }
                else
                {
                    instructions.Emit(OpCodes.Ldtoken, innerMethod);
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    instructions.Emit(OpCodes.Call, ReflectionUtility.GetMethodInfo(() => MethodBase.GetMethodFromHandle(new RuntimeMethodHandle())));
                }
            }
            else
                instructions.Emit(OpCodes.Ldnull);

            // abstracted target
            instructions.Emit(abstractedTarget ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

            if (genericParametersVariable != null)
                instructions.EmitLdloc(genericParametersVariable);
            else
                instructions.Emit(OpCodes.Ldnull);

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
                Logger.WriteDebug("Weaving type '{0}' for info", typeDefinition.FullName);
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
                Logger.WriteError("Error while weaving method '{0}': {1}", method.FullName, e);
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
            Logger.WriteDebug("Weaving interface '{0}'", interfaceType.FullName);
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
                var advisedInterfaceTypeReference = moduleDefinition.SafeImport(advisedInterfaceType);
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
                        Logger.WriteDebug("Introduced field type '{0}'", introducedFieldType.FullName);
                        var introducedFieldTypeReference = moduleDefinition.SafeImport(introducedFieldType);
                        var introducedField = new FieldDefUser(introducedFieldName, new FieldSig(introducedFieldTypeReference.ToTypeSig()), fieldAttributes);
                        introducedField.CustomAttributes.Add(new CustomAttribute(markerAttribute));
                        advisedType.Fields.Add(introducedField);
                    }
                }
            }
        }
    }
}
