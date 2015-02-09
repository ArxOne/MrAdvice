#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Weaver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using Introduction;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Reflection;
    using Utility;
    using FieldAttributes = Mono.Cecil.FieldAttributes;
    using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
    using MethodAttributes = Mono.Cecil.MethodAttributes;

    /// <summary>
    /// Aspect weaver core
    /// Pointcuts are identified here
    /// Advices are injected here
    /// </summary>
    internal class AspectWeaver
    {
        public Logger Logger { get; set; }
        public TypeResolver TypeResolver { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether all additional methods and fields are injected as prived.
        /// Just because Silverlight does not like invoking private methods or fields, even by reflection
        /// </summary>
        /// <value>
        ///   <c>true</c> if [inject as private]; otherwise, <c>false</c>.
        /// </value>
        public bool InjectAsPrivate { get; set; }

        private readonly IDictionary<Tuple<string, string>, bool> _isInterface = new Dictionary<Tuple<string, string>, bool>();

        /// <summary>
        /// Weaves the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        public void Weave(ModuleDefinition moduleDefinition)
        {
            // sanity check
            var adviceInterface = TypeResolver.Resolve(moduleDefinition, Binding.AdviceInterfaceName);
            if (adviceInterface == null)
            {
                Logger.WriteWarning("IAdvice interface not found here, exiting");
                return;
            }
            // runtime check
            var targetFramework = GetTargetFramework(moduleDefinition);
            Logger.Write("Assembly '{0}' targets framework {1}", moduleDefinition.Assembly.FullName, targetFramework);
            InjectAsPrivate = targetFramework.Silverlight == null && targetFramework.WindowsPhone == null;

            // weave methods (they can be property-related, too)
            var weavableMethods = GetMethods(moduleDefinition, adviceInterface).ToArray();
            foreach (var method in weavableMethods)
            {
                WeaveAdvices(method);
                WeaveIntroductions(method, adviceInterface);
            }

            // and then, the info advices
            var infoAdviceInterface = TypeResolver.Resolve(moduleDefinition, Binding.InfoAdviceInterfaceName);
            if (GetMethods(moduleDefinition, infoAdviceInterface).Any())
                WeaveInfoAdvices(moduleDefinition);
        }

        /// <summary>
        /// Gets the target framework.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        private static TargetFramework GetTargetFramework(ModuleDefinition moduleDefinition)
        {
            var targetFrameworkAttributeType = moduleDefinition.Import(typeof(TargetFrameworkAttribute));
            var targetFrameworkAttribute = moduleDefinition.Assembly.CustomAttributes.SingleOrDefault(a => a.AttributeType.SafeEquivalent(targetFrameworkAttributeType));
            if (targetFrameworkAttribute == null)
            {
                switch (moduleDefinition.Runtime)
                {
                    case TargetRuntime.Net_1_0:
                        return new TargetFramework(new Version(1, 0));
                    case TargetRuntime.Net_1_1:
                        return new TargetFramework(new Version(1, 1));
                    case TargetRuntime.Net_2_0:
                        return new TargetFramework(new Version(2, 0));
                    case TargetRuntime.Net_4_0:
                        return new TargetFramework(new Version(4, 0));
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new TargetFramework((string)targetFrameworkAttribute.ConstructorArguments[0].Value);
        }

        /// <summary>
        /// Weaves the introductions.
        /// Introduces members as requested by aspects
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="adviceInterface">The advice interface.</param>
        private void WeaveIntroductions(MethodDefinition method, TypeDefinition adviceInterface)
        {
            var typeDefinition = method.DeclaringType;
            var advices = GetAllAdvices(method, adviceInterface);
            foreach (var advice in advices)
            {
                var adviceDefinition = advice.Resolve();
                foreach (var field in adviceDefinition.Fields)
                    IntroduceMember(method.Module, field.Name, field.FieldType, field.IsStatic, advice, typeDefinition);
                foreach (var property in adviceDefinition.Properties)
                    IntroduceMember(method.Module, property.Name, property.PropertyType, !property.HasThis, advice, typeDefinition);
            }
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
        private void IntroduceMember(ModuleDefinition moduleDefinition, string memberName, TypeReference memberType, bool isStatic, TypeReference adviceType, TypeDefinition advisedType)
        {
            TypeReference introducedFieldType;
            if (IsIntroduction(moduleDefinition, memberType, out introducedFieldType))
            {
                var introducedFieldName = IntroductionRules.GetName(adviceType.Namespace, adviceType.Name, memberName);
                if (advisedType.Fields.All(f => f.Name != introducedFieldName))
                {
                    var fieldAttributes = (InjectAsPrivate ? FieldAttributes.Private : FieldAttributes.Public) | FieldAttributes.NotSerialized;
                    if (isStatic)
                        fieldAttributes |= FieldAttributes.Static;
                    var introducedField = new FieldDefinition(introducedFieldName,
                       fieldAttributes, moduleDefinition.Import(introducedFieldType));
                    advisedType.Fields.Add(introducedField);
                }
            }
        }

        /// <summary>
        /// Determines whether the advice member is introduction, based on its type.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="adviceMemberTypeReference">The type reference.</param>
        /// <param name="introducedFieldType">Type of the introduced field.</param>
        /// <returns></returns>
        private bool IsIntroduction(ModuleDefinition moduleDefinition, TypeReference adviceMemberTypeReference, out TypeReference introducedFieldType)
        {
            if (!adviceMemberTypeReference.IsGenericInstance)
            {
                introducedFieldType = null;
                return false;
            }
            var index = adviceMemberTypeReference.FullName.IndexOf('<');
            var genericTypeName = adviceMemberTypeReference.FullName.Substring(0, index);
            if (genericTypeName != Binding.IntroducedFieldTypeName)
            {
                introducedFieldType = null;
                return false;
            }

            var introducedFieldTypeName = adviceMemberTypeReference.FullName.Substring(index + 1, adviceMemberTypeReference.FullName.Length - index - 2);
            introducedFieldType = TypeResolver.Resolve(moduleDefinition, introducedFieldTypeName);
            return true;
        }

        /// <summary>
        /// Weaves the runtime initializers for the given module.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        private void WeaveInfoAdvices(ModuleDefinition moduleDefinition)
        {
            var moduleType = moduleDefinition.Types.Single(t => t.Name == "<Module>");

            const string cctorMethodName = ".cctor";
            var staticCtor = moduleType.Methods.SingleOrDefault(m => m.Name == cctorMethodName);
            if (staticCtor == null)
            {
                staticCtor = new MethodDefinition(cctorMethodName,
                           (InjectAsPrivate ? MethodAttributes.Private : MethodAttributes.Public)
                           | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                           moduleDefinition.Import(typeof(void)));
                moduleType.Methods.Add(staticCtor);
            }

            var instructions = new Instructions(staticCtor.Body.Instructions);

            var invocationType = TypeResolver.Resolve(moduleDefinition, Binding.InvocationTypeName);
            if (invocationType == null)
                return;
            var proceedRuntimeInitializersReference = invocationType.GetMethods().SingleOrDefault(m => m.IsStatic && m.Name == Binding.InvocationProcessRuntimeInitializersMethodName);
            if (proceedRuntimeInitializersReference == null)
                return;
            var proceedMethod = moduleDefinition.Import(proceedRuntimeInitializersReference);

            instructions.Emit(OpCodes.Call, moduleDefinition.Import(ReflectionUtility.GetMethodInfo(() => Assembly.GetExecutingAssembly())));
            instructions.Emit(OpCodes.Call, proceedMethod);
            instructions.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Weaves the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        private void WeaveAdvices(MethodDefinition method)
        {
            Logger.WriteDebug("Weaving method '{0}'", method.FullName);

            var moduleDefinition = method.DeclaringType.Module;

            // create inner method
            const MethodAttributes attributesToKeep = MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.PInvokeImpl |
                                                      MethodAttributes.UnmanagedExport | MethodAttributes.HasSecurity |
                                                      MethodAttributes.RequireSecObject;
            var innerMethodAttributes = method.Attributes & attributesToKeep | (InjectAsPrivate ? MethodAttributes.Private : MethodAttributes.Public);
            string innerMethodName;
            if (method.IsGetter)
                innerMethodName = string.Format("\u200B{0}.get", method.Name.Substring(4));
            else if (method.IsSetter)
                innerMethodName = string.Format("\u200B{0}.set", method.Name.Substring(4));
            else
                innerMethodName = string.Format("{0}\u200B", method.Name);
            var innerMethod = new MethodDefinition(innerMethodName, innerMethodAttributes, method.ReturnType);
            innerMethod.GenericParameters.AddRange(method.GenericParameters.Select(p => p.Clone(innerMethod)));
            innerMethod.ImplAttributes = method.ImplAttributes;
            innerMethod.SemanticsAttributes = method.SemanticsAttributes;
            innerMethod.Body.InitLocals = false;//method.Body.InitLocals;
            innerMethod.Parameters.AddRange(method.Parameters);
            innerMethod.Body.Instructions.AddRange(method.Body.Instructions);
            innerMethod.Body.Variables.AddRange(method.Body.Variables);
            innerMethod.Body.ExceptionHandlers.AddRange(method.Body.ExceptionHandlers);

            // now empty the old one and make it call the inner method...
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
            method.Body.ExceptionHandlers.Clear();
            var instructions = new Instructions(method.Body.Instructions);

            var isStatic = method.Attributes.HasFlag(MethodAttributes.Static);
            var firstParameter = isStatic ? 0 : 1;

            // parameters
            var parametersVariable = new VariableDefinition("parameters", moduleDefinition.Import(typeof(object[])));
            method.Body.Variables.Add(parametersVariable);

            instructions.EmitLdc(method.Parameters.Count);
            instructions.Emit(OpCodes.Newarr, moduleDefinition.Import(typeof(object)));
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
                    instructions.EmitLdarg(parameterIndex + firstParameter); // loads given parameter...
                    var parameterType = parameter.ParameterType;
                    if (parameter.ParameterType.IsByReference) // ...if ref, loads it as referenced value
                    {
                        parameterType = GetReferencedType(moduleDefinition, parameter.ParameterType);
                        instructions.EmitLdind(parameterType);
                    }
                    instructions.EmitBoxIfNecessary(parameterType); // ... and boxes it
                    instructions.Emit(OpCodes.Stelem_Ref);
                }
            }
            // null or instance
            instructions.Emit(isStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);

            // parameters
            instructions.EmitLdloc(parametersVariable);

            // methods...
            // ...target
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            instructions.Emit(OpCodes.Call, moduleDefinition.Import(ReflectionUtility.GetMethodInfo(() => MethodBase.GetCurrentMethod())));

            // ...inner
            var actionType = moduleDefinition.Import(typeof(Action));
            var actionCtor = moduleDefinition.Import(actionType.Resolve().GetConstructors().Single());

            var delegateType = moduleDefinition.Import(typeof(Delegate));
            var getMethod = moduleDefinition.Import(delegateType.Resolve().Methods.Single(m => m.Name == "get_Method"));

            // ...inner
            instructions.Emit(isStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);
            instructions.Emit(OpCodes.Ldftn, innerMethod);
            instructions.Emit(OpCodes.Newobj, actionCtor);
            instructions.Emit(OpCodes.Call, getMethod);

            // invoke the method
            var invocationType = TypeResolver.Resolve(moduleDefinition, Binding.InvocationTypeName);
            if (invocationType == null)
                return;
            var proceedMethodReference = invocationType.GetMethods().SingleOrDefault(m => m.IsStatic && m.Name == Binding.InvocationProceedMethodMethodName);
            if (proceedMethodReference == null)
                return;
            var proceedMethod = moduleDefinition.Import(proceedMethodReference);

            instructions.Emit(OpCodes.Call, proceedMethod);

            // get return value
            if (!method.ReturnType.SafeEquivalent(moduleDefinition.Import(typeof(void))))
                instructions.EmitUnboxOrCastIfNecessary(method.ReturnType);
            else
                instructions.Emit(OpCodes.Pop); // if no return type, ignore Proceed() result

            // loads back out/ref parameters
            for (int parameterIndex = 0; parameterIndex < method.Parameters.Count; parameterIndex++)
            {
                var parameter = method.Parameters[parameterIndex];
                if (parameter.ParameterType.IsByReference)
                {
                    instructions.EmitLdarg(parameterIndex + firstParameter); // loads given parameter (it is a ref)
                    instructions.EmitLdloc(parametersVariable); // array
                    instructions.EmitLdc(parameterIndex); // array index
                    instructions.Emit(OpCodes.Ldelem_Ref); // now we have boxed out/ref value
                    instructions.EmitUnboxOrCastIfNecessary(GetReferencedType(moduleDefinition, parameter.ParameterType));
                    instructions.EmitStind(parameter.ParameterType); // result is stored in ref parameter
                }
            }

            // and return
            instructions.Emit(OpCodes.Ret);

            method.DeclaringType.Methods.Add(innerMethod);
        }

        /// <summary>
        /// Gets the type of the referenced type.
        /// (isolates all the crap)
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="referenceType">Type of the reference.</param>
        /// <returns></returns>
        private TypeReference GetReferencedType(ModuleDefinition moduleDefinition, TypeReference referenceType)
        {
            // not sure this is the best way to do things, but it is a working way anyway
            return moduleDefinition.Import(TypeResolver.Resolve(moduleDefinition, referenceType.FullName.TrimEnd('&')));
        }

        /// <summary>
        /// Gets all weavable methods from module.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="markerInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private IEnumerable<MethodDefinition> GetMethods(ModuleDefinition moduleDefinition, TypeDefinition markerInterface)
        {
            foreach (var typeDefinition in moduleDefinition.GetTypes())
            {
                // methods
                foreach (var methodDefinition in typeDefinition.GetMethods())
                {
                    if (GetAllAdvices(methodDefinition, markerInterface).Any())
                    {
                        if (typeDefinition.HasGenericParameters || methodDefinition.HasGenericParameters)
                        {
                            Logger.WriteWarning("Generic method {0} can not be weaved", methodDefinition.FullName);
                            continue;
                        }
                        yield return methodDefinition;
                    }
                }
                // ctors
                foreach (var ctorDefinition in typeDefinition.GetConstructors())
                {
                    if (GetAllAdvices(ctorDefinition, markerInterface).Any())
                        yield return ctorDefinition;
                }
                // properties have methods too
                foreach (var propertyDefinition in typeDefinition.Properties)
                {
                    if (GetAllAdvices(propertyDefinition, markerInterface).Any())
                    {
                        if (propertyDefinition.GetMethod != null)
                            yield return propertyDefinition.GetMethod;
                        if (propertyDefinition.SetMethod != null)
                            yield return propertyDefinition.SetMethod;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all advices, applied at member level, type level and assembly level.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <param name="adviceInterface">The advice interface.</param>
        /// <returns></returns>
        private IEnumerable<TypeReference> GetAllAdvices(IMemberDefinition methodDefinition, TypeDefinition adviceInterface)
        {
            return GetAdvices(methodDefinition.DeclaringType.Module.Assembly, adviceInterface)
                .Concat(GetAdvices(methodDefinition.DeclaringType, adviceInterface))
                .Concat(GetAdvices(methodDefinition, adviceInterface))
                .Distinct();
        }

        /// <summary>
        /// Determines whether the specified method (attribute provider) has aspects, given a marker.
        /// It searches through all attributes to find one implementing the marker
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        /// <param name="adviceInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private IEnumerable<TypeReference> GetAdvices(ICustomAttributeProvider attributeProvider, TypeDefinition adviceInterface)
        {
            return attributeProvider.CustomAttributes.Select(a => a.AttributeType).Where(t => IsAdvice(t, adviceInterface));
        }

        /// <summary>
        /// Determines whether the specified type reference is aspect.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <param name="adviceInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private bool IsAdvice(TypeReference typeReference, TypeDefinition adviceInterface)
        {
            var key = Tuple.Create(typeReference.FullName, adviceInterface.FullName);
            // there is a cache, because the same attribute may be found several time
            // and we're in a hurry, the developper is waiting for his program to start!
            bool isAspect;
            if (_isInterface.TryGetValue(key, out isAspect))
                return isAspect;

            // otherwise look for type or implemented interfaces (recursively)
            var interfaces = typeReference.Resolve().Interfaces;
            _isInterface[key] = isAspect = typeReference.SafeEquivalent(adviceInterface)
                || interfaces.Any(i => IsAdvice(i, adviceInterface));
            return isAspect;
        }
    }
}
