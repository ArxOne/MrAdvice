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
    using System.Runtime.InteropServices;
    using Annotation;
    using Introduction;
    using IO;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Reflection.Groups;
    using Utility;
    using EventAttributes = Mono.Cecil.EventAttributes;
    using FieldAttributes = Mono.Cecil.FieldAttributes;
    using MethodAttributes = Mono.Cecil.MethodAttributes;
    using MethodBody = Mono.Cecil.Cil.MethodBody;
    using PropertyAttributes = Mono.Cecil.PropertyAttributes;
    using TypeAttributes = Mono.Cecil.TypeAttributes;

    partial class AspectWeaver
    {
        /// <summary>
        /// Weaves the info advices for the given type.
        /// </summary>
        /// <param name="infoAdvisedType">Type of the module.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="useWholeAssembly">if set to <c>true</c> [use whole assembly].</param>
        private void WeaveInfoAdvices(TypeDefinition infoAdvisedType, ModuleDefinition moduleDefinition, bool useWholeAssembly)
        {
            var invocationType = TypeResolver.Resolve(moduleDefinition, Binding.InvocationTypeName, true);
            if (invocationType == null)
                return;
            var proceedRuntimeInitializersReference = (from m in invocationType.GetMethods()
                                                       where m.IsStatic && m.Name == Binding.InvocationProcessInfoAdvicesMethodName
                                                       let parameters = m.Parameters
                                                       where parameters.Count == 1
                                                             && parameters[0].ParameterType.SafeEquivalent(
                                                                 moduleDefinition.SafeImport(useWholeAssembly ? typeof(Assembly) : typeof(Type)))
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
            if (staticCtor == null)
            {
                staticCtor = new MethodDefinition(cctorMethodName,
                    (InjectAsPrivate ? MethodAttributes.Private : MethodAttributes.Public)
                    | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    moduleDefinition.SafeImport(typeof(void)));
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

                var weavingAdvices = GetAllMarkers(markedMethod.Node, types.WeavingAdviceAttributeType, types).ToArray();
                if (weavingAdvices.Any())
                {
                    Logger.Write("{0} weaving advice(s) {1}", weavingAdvices.Length,types.WeavingAdviceAttributeType.FullName);
                }

                // create inner method
                const MethodAttributes attributesToKeep = MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.PInvokeImpl |
                                                          MethodAttributes.UnmanagedExport | MethodAttributes.HasSecurity |
                                                          MethodAttributes.RequireSecObject;
                var innerMethodAttributes = method.Attributes & attributesToKeep |
                                            (InjectAsPrivate ? MethodAttributes.Private : MethodAttributes.Public);
                string innerMethodName;
                if (method.IsGetter)
                    innerMethodName = GetPropertyInnerGetterName(GetPropertyName(method.Name));
                else if (method.IsSetter)
                    innerMethodName = GetPropertyInnerSetterName(GetPropertyName(method.Name));
                else
                    innerMethodName = GetInnerMethodName(method.Name);
                var innerMethod = new MethodDefinition(innerMethodName, innerMethodAttributes, method.ReturnType);
                innerMethod.GenericParameters.AddRange(method.GenericParameters.Select(p => p.Clone(innerMethod)));
                innerMethod.ImplAttributes = method.ImplAttributes;
                innerMethod.SemanticsAttributes = method.SemanticsAttributes;
                innerMethod.Parameters.AddRange(method.Parameters);
                if (method.IsPInvokeImpl)
                {
                    innerMethod.PInvokeInfo = method.PInvokeInfo;
                    // must be removed before attributes are updated (otherwise Cecil gets angry)
                    method.PInvokeInfo = null;
                    method.IsPreserveSig = false;
                    method.IsPInvokeImpl = false;
                }
                else
                {
                    innerMethod.Body.InitLocals = method.Body.InitLocals;
                    innerMethod.Body.Instructions.AddRange(method.Body.Instructions);
                    innerMethod.Body.Variables.AddRange(method.Body.Variables);
                    innerMethod.Body.ExceptionHandlers.AddRange(method.Body.ExceptionHandlers);
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
        private void WritePointcutBody(MethodDefinition method, MethodDefinition innerMethod, bool abstractedTarget)
        {
            var moduleDefinition = method.Module;

            // now empty the old one and make it call the inner method...
            if (method.Body == null)
                method.Body = new MethodBody(method);
            method.Body.InitLocals = true;
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
            method.Body.ExceptionHandlers.Clear();
            var instructions = new Instructions(method.Body.Instructions, method.Module);

            var isStatic = method.Attributes.HasFlag(MethodAttributes.Static);

            // parameters
            VariableDefinition parametersVariable = null;
            if (method.Parameters.Count > 0)
            {
                parametersVariable = new VariableDefinition("parameters", moduleDefinition.SafeImport(typeof(object[])));
                method.Body.Variables.Add(parametersVariable);

                instructions.EmitLdc(method.Parameters.Count);
                instructions.Emit(OpCodes.Newarr, moduleDefinition.SafeImport(typeof(object)));
                instructions.EmitStloc(parametersVariable);
                // setups parameters array
                for (int parameterIndex = 0; parameterIndex < method.Parameters.Count; parameterIndex++)
                {
                    var parameter = method.Parameters[parameterIndex];
                    // we don't care about output parameters
                    if (!parameter.IsOut)
                    {
                        instructions.EmitLdloc(parametersVariable); // array
                        instructions.EmitLdc(parameterIndex); // array index
                        instructions.EmitLdarg(parameter); // loads given parameter...
                        var parameterType = parameter.ParameterType;
                        if (parameter.ParameterType.IsByReference) // ...if ref, loads it as referenced value
                        {
                            parameterType = parameter.ParameterType.GetElementType();
                            instructions.EmitLdind(parameterType);
                        }
                        instructions.EmitBoxIfNecessary(parameterType); // ... and boxes it
                        instructions.Emit(OpCodes.Stelem_Ref);
                    }
                }
            }

            // if method has generic parameters, we also pass them to Proceed method
            VariableDefinition genericParametersVariable = null;
            // on static methods from genetic type, we also record the generic parameters type
            //var typeGenericParametersCount = isStatic ? method.DeclaringType.GenericParameters.Count : 0;
            var typeGenericParametersCount = method.DeclaringType.GenericParameters.Count;
            if (typeGenericParametersCount > 0 || method.HasGenericParameters)
            {
                //IL_0001: ldtoken !!T
                //IL_0006: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
                genericParametersVariable = new VariableDefinition("genericParameters", moduleDefinition.SafeImport(typeof(Type[])));
                method.Body.Variables.Add(genericParametersVariable);

                instructions.EmitLdc(typeGenericParametersCount + method.GenericParameters.Count);
                instructions.Emit(OpCodes.Newarr, moduleDefinition.SafeImport(typeof(Type)));
                instructions.EmitStloc(genericParametersVariable);

                var genericParameters = new List<GenericParameter>();
                for (int typeGenericParameterIndex = 0; typeGenericParameterIndex < typeGenericParametersCount; typeGenericParameterIndex++)
                    genericParameters.Add(method.DeclaringType.GenericParameters[typeGenericParameterIndex]);
                genericParameters.AddRange(method.GenericParameters);

                for (int genericParameterIndex = 0; genericParameterIndex < genericParameters.Count; genericParameterIndex++)
                {
                    instructions.EmitLdloc(genericParametersVariable); // array
                    instructions.EmitLdc(genericParameterIndex); // array index
                    instructions.Emit(OpCodes.Ldtoken, genericParameters[genericParameterIndex]);
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    instructions.Emit(OpCodes.Call, ReflectionUtility.GetMethodInfo(() => Type.GetTypeFromHandle(new RuntimeTypeHandle())));
                    instructions.Emit(OpCodes.Stelem_Ref);
                }
            }

            // null or instance
            instructions.Emit(isStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);
            // to fix peverify 0x80131854
            if (!isStatic && method.IsConstructor)
                instructions.Emit(OpCodes.Castclass, typeof(object));

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
                    var currentMethodVariable = new VariableDefinition("currentMethod", moduleDefinition.SafeImport(typeof(MethodBase)));
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
            var invocationType = TypeResolver.Resolve(moduleDefinition, Binding.InvocationTypeName, true);
            if (invocationType == null)
                throw new InvalidOperationException();
            var proceedMethodReference = invocationType.GetMethods().SingleOrDefault(m => m.IsStatic && m.Name == Binding.InvocationProceedAdviceMethodName);
            if (proceedMethodReference == null)
                throw new InvalidOperationException();
            var proceedMethod = moduleDefinition.SafeImport(proceedMethodReference);

            instructions.Emit(OpCodes.Call, proceedMethod);

            // get return value
            if (!method.ReturnType.SafeEquivalent(moduleDefinition.SafeImport(typeof(void))))
                instructions.EmitUnboxOrCastIfNecessary(method.ReturnType);
            else
                instructions.Emit(OpCodes.Pop); // if no return type, ignore Proceed() result

            // loads back out/ref parameters
            for (int parameterIndex = 0; parameterIndex < method.Parameters.Count; parameterIndex++)
            {
                var parameter = method.Parameters[parameterIndex];
                if (parameter.ParameterType.IsByReference)
                {
                    instructions.EmitLdarg(parameter); // loads given parameter (it is a ref)
                    instructions.EmitLdloc(parametersVariable); // array
                    instructions.EmitLdc(parameterIndex); // array index
                    instructions.Emit(OpCodes.Ldelem_Ref); // now we have boxed out/ref value
                    var parameterElementType = parameter.ParameterType.GetElementType();
                    instructions.EmitUnboxOrCastIfNecessary(parameterElementType);
                    instructions.EmitStind(parameterElementType); // result is stored in ref parameter
                }
            }

            // and return
            instructions.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Weaves the introductions.
        /// Introduces members as requested by aspects
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="adviceInterface">The advice interface.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="types">The types.</param>
        private void WeaveIntroductions(MethodDefinition method, TypeDefinition adviceInterface, ModuleDefinition moduleDefinition, Types types)
        {
            var typeDefinition = method.DeclaringType;
            var advices = GetAllMarkers(new MethodReflectionNode(method), adviceInterface, types);
            var markerAttributeCtor = moduleDefinition.SafeImport(TypeResolver.Resolve(moduleDefinition, Binding.IntroducedFieldAttributeName, true)
                .GetConstructors().Single());
            foreach (var advice in advices)
            {
                var adviceDefinition = advice.Type.Resolve();
                foreach (var field in adviceDefinition.Fields.Where(f => f.IsPublic))
                    IntroduceMember(method.Module, field.Name, field.FieldType, field.IsStatic, advice.Type, typeDefinition, markerAttributeCtor);
                foreach (var property in adviceDefinition.Properties.Where(p => p.HasAnyPublic()))
                    IntroduceMember(method.Module, property.Name, property.PropertyType, !property.HasThis, advice.Type, typeDefinition, markerAttributeCtor);
            }
        }

        /// <summary>
        /// Weaves the information advices.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="infoAdviceInterface">The information advice interface.</param>
        /// <param name="types">The types.</param>
        private void WeaveInfoAdvices(ModuleDefinition moduleDefinition, TypeDefinition typeDefinition, TypeDefinition infoAdviceInterface, Types types)
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
        private void WeaveMethod(ModuleDefinition moduleDefinition, MarkedNode markedMethod, TypeDefinition adviceInterface, Types types)
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
        private void WeaveInterface(ModuleDefinition moduleDefinition, TypeReference interfaceType)
        {
            Logger.WriteDebug("Weaving interface '{0}'", interfaceType.FullName);
            TypeDefinition implementationType;
            TypeDefinition advisedInterfaceType;
            TypeDefinition interfaceTypeDefinition;
            lock (moduleDefinition)
            {
                // ensure we're creating the interface only once
                var implementationTypeName = GetImplementationTypeName(interfaceType.Name);
                var implementationTypeNamespace = interfaceType.Namespace;
                if (moduleDefinition.GetTypes().Any(t => t.Namespace == implementationTypeNamespace && t.Name == implementationTypeName))
                    return;

                // now, create the implementation type
                interfaceTypeDefinition = interfaceType.Resolve();
                var typeAttributes = (InjectAsPrivate ? TypeAttributes.NotPublic : TypeAttributes.Public) | TypeAttributes.Class | TypeAttributes.BeforeFieldInit;
                advisedInterfaceType = TypeResolver.Resolve(moduleDefinition, Binding.AdvisedInterfaceTypeName, true);
                var advisedInterfaceTypeReference = moduleDefinition.SafeImport(advisedInterfaceType);
                implementationType = new TypeDefinition(implementationTypeNamespace, implementationTypeName, typeAttributes, advisedInterfaceTypeReference);

                lock (moduleDefinition)
                    moduleDefinition.Types.Add(implementationType);
            }

            implementationType.Interfaces.Add(interfaceType);

            // create empty .ctor. This .NET mofo wants it!
            var baseEmptyConstructor = moduleDefinition.SafeImport(advisedInterfaceType.Resolve().GetConstructors().Single());
            const MethodAttributes ctorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var method = new MethodDefinition(".ctor", ctorAttributes, moduleDefinition.TypeSystem.Void);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, baseEmptyConstructor));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            implementationType.Methods.Add(method);

            // create implementation methods
            foreach (var interfaceMethod in interfaceTypeDefinition.GetMethods().Where(m => !m.IsSpecialName))
                WeaveInterfaceMethod(interfaceMethod, implementationType, true);

            // create implementation properties
            foreach (var interfaceProperty in interfaceTypeDefinition.Properties)
            {
                var implementationProperty = new PropertyDefinition(interfaceProperty.Name, PropertyAttributes.None, interfaceProperty.PropertyType);
                implementationType.Properties.Add(implementationProperty);
                if (interfaceProperty.GetMethod != null)
                    implementationProperty.GetMethod = WeaveInterfaceMethod(interfaceProperty.GetMethod, implementationType, InjectAsPrivate);
                if (interfaceProperty.SetMethod != null)
                    implementationProperty.SetMethod = WeaveInterfaceMethod(interfaceProperty.SetMethod, implementationType, InjectAsPrivate);
            }

            // create implementation events
            foreach (var interfaceEvent in interfaceTypeDefinition.Events)
            {
                var implementationEvent = new EventDefinition(interfaceEvent.Name, EventAttributes.None, moduleDefinition.SafeImport(interfaceEvent.EventType));
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
        private MethodDefinition WeaveInterfaceMethod(MethodDefinition interfaceMethod, TypeDefinition implementationType, bool injectAsPrivate)
        {
            var methodAttributes = MethodAttributes.NewSlot | MethodAttributes.Virtual | (injectAsPrivate ? MethodAttributes.Public : MethodAttributes.Private);
            var implementationMethod = new MethodDefinition(interfaceMethod.Name, methodAttributes, interfaceMethod.ReturnType);
            implementationType.Methods.Add(implementationMethod);
            implementationMethod.HasThis = interfaceMethod.HasThis;
            implementationMethod.ExplicitThis = interfaceMethod.ExplicitThis;
            implementationMethod.CallingConvention = interfaceMethod.CallingConvention;
            implementationMethod.IsSpecialName = interfaceMethod.IsSpecialName;
            implementationMethod.Parameters.AddRange(interfaceMethod.Parameters);
            implementationMethod.GenericParameters.AddRange(interfaceMethod.GenericParameters);
            implementationMethod.Overrides.Add(interfaceMethod);
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
        /// <param name="markerAttributeCtor">The marker attribute ctor.</param>
        private void IntroduceMember(ModuleDefinition moduleDefinition, string memberName, TypeReference memberType, bool isStatic,
            TypeReference adviceType, TypeDefinition advisedType, MethodReference markerAttributeCtor)
        {
            TypeReference introducedFieldType;
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
                        var introducedField = new FieldDefinition(introducedFieldName, fieldAttributes, introducedFieldTypeReference);
                        introducedField.CustomAttributes.Add(new CustomAttribute(markerAttributeCtor));
                        advisedType.Fields.Add(introducedField);
                    }
                }
            }
        }
    }
}
