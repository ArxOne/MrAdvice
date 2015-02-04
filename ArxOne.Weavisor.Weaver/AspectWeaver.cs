#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Weaver
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Utility;
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
        /// Weaves the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        public void Weave(ModuleDefinition moduleDefinition)
        {
            // TODO not sure we'll need to keep the IAdvice interface
            var adviceInterface = TypeResolver.Resolve(moduleDefinition, Binding.AdviceInterfaceName);
            if (adviceInterface == null)
            {
                Logger.WriteWarning("IAdvice interface not found here, exiting");
                return;
            }
            var weavableMethods = GetMethods(moduleDefinition, TypeResolver.Resolve(moduleDefinition, Binding.AdviceInterfaceName)).ToArray();
            foreach (var method in weavableMethods)
                Weave(method);

            var initializerInterface = TypeResolver.Resolve(moduleDefinition, Binding.InitializerInterfaceName);
            if (GetMethods(moduleDefinition, initializerInterface).Any())
                WeaveRuntimeInitializer(moduleDefinition);
        }

        /// <summary>
        /// Weaves the runtime initializers for the given module.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        private void WeaveRuntimeInitializer(ModuleDefinition moduleDefinition)
        {
            var moduleType = moduleDefinition.Types.Single(t => t.Name == "<Module>");

            const string cctorMethodName = ".cctor";
            var staticCtor = moduleType.Methods.SingleOrDefault(m => m.Name == cctorMethodName);
            if (staticCtor == null)
            {
                staticCtor = new MethodDefinition(cctorMethodName,
                           MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                           MethodAttributes.RTSpecialName,
                           moduleDefinition.Import(typeof(void)));
                moduleType.Methods.Add(staticCtor);
            }

            var instructions = new Instructions(staticCtor.Body.Instructions);

            var invocationType = TypeResolver.Resolve(moduleDefinition, Binding.InvocationProceedTypeName);
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
        private void Weave(MethodDefinition method)
        {
            Logger.WriteDebug("Weaving method '{0}'", method.FullName);

            var moduleDefinition = method.DeclaringType.Module;

            // create inner method
            Logger.WriteDebug("> attributes '{0}'", method.Attributes);
            const MethodAttributes attributesToKeep = MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.PInvokeImpl |
                                                      MethodAttributes.UnmanagedExport | MethodAttributes.HasSecurity |
                                                      MethodAttributes.RequireSecObject;
            var innerMethodAttributes = method.Attributes & attributesToKeep | MethodAttributes.Private;
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

            // now empty the old one and make it call the inner method...
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
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
                    if (parameter.IsIn && parameter.IsOut) // ...if ref, loads it as referenced value
                        instructions.EmitLdind(parameter.ParameterType);
                    instructions.EmitBoxIfNecessary(parameter.ParameterType); // ... and boxes it
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
            instructions.Emit(OpCodes.Ldstr, innerMethodName);

            // invoke the method
            var invocationType = TypeResolver.Resolve(moduleDefinition, Binding.InvocationProceedTypeName);
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
                if (parameter.IsOut)
                {
                    instructions.EmitLdarg(parameterIndex + firstParameter); // loads given parameter (it is a ref)
                    instructions.EmitLdloc(parametersVariable); // array
                    instructions.EmitLdc(parameterIndex); // array index
                    instructions.Emit(OpCodes.Ldelem_Ref); // now we have boxed out/ref value
                    instructions.EmitUnboxOrCastIfNecessary(parameter.ParameterType);
                    instructions.EmitStind(parameter.ParameterType); // result is stored in ref parameter
                }
            }

            // and return
            instructions.Emit(OpCodes.Ret);

            method.DeclaringType.Methods.Add(innerMethod);
        }

        /// <summary>
        /// Gets all weavable methods from module.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="markerInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private IEnumerable<MethodDefinition> GetMethods(ModuleDefinition moduleDefinition, TypeDefinition markerInterface)
        {
            IDictionary<TypeReference, bool> markerCache = new Dictionary<TypeReference, bool>();
            bool weaveAssembly = HasMethodMarkers(moduleDefinition.Assembly, markerInterface, markerCache);
            foreach (var typeDefinition in moduleDefinition.GetTypes())
            {
                var weaveType = HasMethodMarkers(typeDefinition, markerInterface, markerCache);
                if (weaveType && typeDefinition.HasGenericParameters)
                {
                    Logger.WriteWarning("Generic type {0} can not be weaved", typeDefinition.FullName);
                    continue;
                }
                // methods
                foreach (var methodDefinition in typeDefinition.GetMethods())
                {
                    if (weaveAssembly || weaveType || HasMethodMarkers(methodDefinition, markerInterface, markerCache))
                    {
                        if (methodDefinition.HasGenericParameters)
                        {
                            Logger.WriteWarning("Generic method {0} can not be weaved", methodDefinition.FullName, markerCache);
                            continue;
                        }
                        yield return methodDefinition;
                    }
                }
                // ctors
                foreach (var ctorDefinition in typeDefinition.GetConstructors())
                {
                    if (weaveAssembly || weaveType || HasMethodMarkers(ctorDefinition, markerInterface, markerCache))
                        yield return ctorDefinition;
                }
                // properties have methods too
                foreach (var propertyDefinition in typeDefinition.Properties)
                {
                    if (weaveAssembly || weaveType || HasMethodMarkers(propertyDefinition, markerInterface, markerCache))
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
        /// Determines whether the specified method (attribute provider) has aspects, given a marker.
        /// It searches through all attributes to find one implementing the marker
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        /// <param name="aspectMarkerInterface">The aspect marker interface.</param>
        /// <param name="markerInterface">The marker interface.</param>
        /// <returns></returns>
        private bool HasMethodMarkers(ICustomAttributeProvider attributeProvider, TypeDefinition aspectMarkerInterface, IDictionary<TypeReference, bool> markerInterface)
        {
            return attributeProvider.CustomAttributes.Any(a => HasMarker(a.AttributeType, aspectMarkerInterface, markerInterface));
        }

        /// <summary>
        /// Determines whether the specified type reference is aspect.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <param name="aspectMarkerInterface">The aspect marker interface.</param>
        /// <param name="markerInterface">The marker interface.</param>
        /// <returns></returns>
        private bool HasMarker(TypeReference typeReference, TypeDefinition aspectMarkerInterface, IDictionary<TypeReference, bool> markerInterface)
        {
            // there is a cache, because the same attribute may be found several time
            // and we're in a hurry, the developper is waiting for his program to start!
            bool isAspect;
            if (markerInterface.TryGetValue(typeReference, out isAspect))
                return isAspect;

            // otherwise look for type or implemented interfaces (recursively)
            var interfaces = typeReference.Resolve().Interfaces;
            markerInterface[typeReference] = isAspect = typeReference.SafeEquivalent(aspectMarkerInterface)
                || interfaces.Any(i => HasMarker(i, aspectMarkerInterface, markerInterface));
            return isAspect;
        }
    }
}
