#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Weaver
{
    using System;
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
            var aspectMarkerInterface = TypeResolver.Resolve(moduleDefinition, Binding.AdviceMarkerName);
            if (aspectMarkerInterface == null)
            {
                Logger.WriteWarning("Aspect marker interface not found, exiting");
                return;
            }
            foreach (var method in GetWeavableMethods(moduleDefinition, aspectMarkerInterface).ToArray())
                Weave(method);
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
            // first is fun, second is nice: hard to decide which naming style to pick
            //var innerMethodName = string.Format("<{0}>b", method.Name );
            var innerMethodName = string.Format("{0}\u200B", method.Name);
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
            var instructions = method.Body.Instructions;

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
        /// <param name="aspectMarkerInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private IEnumerable<MethodDefinition> GetWeavableMethods(ModuleDefinition moduleDefinition, TypeDefinition aspectMarkerInterface)
        {
            bool weaveAssembly = HasMethodAspects(moduleDefinition.Assembly, aspectMarkerInterface);
            foreach (var typeDefinition in moduleDefinition.GetTypes())
            {
                var weaveType = HasMethodAspects(typeDefinition, aspectMarkerInterface);
                if (weaveType && typeDefinition.HasGenericParameters)
                {
                    Logger.WriteWarning("Generic type {0} can not be weaved", typeDefinition.FullName);
                    continue;
                }
                // methods
                foreach (var methodDefinition in typeDefinition.GetMethods())
                {
                    if (weaveAssembly || weaveType || HasMethodAspects(methodDefinition, aspectMarkerInterface))
                    {
                        if (methodDefinition.HasGenericParameters)
                        {
                            Logger.WriteWarning("Generic method {0} can not be weaved", methodDefinition.FullName);
                            continue;
                        }
                        yield return methodDefinition;
                    }
                }
                // properties have methods too
                foreach (var propertyDefinition in typeDefinition.Properties)
                {
                    if (weaveAssembly || weaveType || HasMethodAspects(propertyDefinition, aspectMarkerInterface))
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
        /// <returns></returns>
        private bool HasMethodAspects(ICustomAttributeProvider attributeProvider, TypeDefinition aspectMarkerInterface)
        {
            return attributeProvider.CustomAttributes.Any(a => IsAspect(a.AttributeType, aspectMarkerInterface));
        }

        private readonly IDictionary<TypeReference, bool> _isAspect = new Dictionary<TypeReference, bool>();

        /// <summary>
        /// Determines whether the specified type reference is aspect.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <param name="aspectMarkerInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private bool IsAspect(TypeReference typeReference, TypeDefinition aspectMarkerInterface)
        {
            // there is a cache, because the same attribute may be found several time
            // and we're in a hurry, the developper is waiting for his program to start!
            bool isAspect;
            if (_isAspect.TryGetValue(typeReference, out isAspect))
                return isAspect;

            // otherwise look for type or implemented interfaces (recursively)
            var interfaces = typeReference.Resolve().Interfaces;
            _isAspect[typeReference] = isAspect = typeReference.SafeEquivalent(aspectMarkerInterface)
                || interfaces.Any(i => IsAspect(i, aspectMarkerInterface));
            return isAspect;
        }
    }
}
