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
    using IO;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Reflection.Groups;
    using Utility;
    using MethodAttributes = Mono.Cecil.MethodAttributes;

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
                Logger.WriteWarning("Info advice method not foud");
                return;
            }

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
                var getTypeFromHandleMethodInfo = ReflectionUtility.GetMethodInfo(() => Type.GetTypeFromHandle(new RuntimeTypeHandle()));
                instructions.Emit(OpCodes.Call, moduleDefinition.SafeImport(getTypeFromHandleMethodInfo));
            }
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
                innerMethodName = GetPropertyInnerGetterName(GetPropertyName(method.Name));
            else if (method.IsSetter)
                innerMethodName = GetPropertyInnerSetterName(GetPropertyName(method.Name));
            else
                innerMethodName = GetInnerMethodName(method.Name);
            var innerMethod = new MethodDefinition(innerMethodName, innerMethodAttributes, method.ReturnType);
            innerMethod.GenericParameters.AddRange(method.GenericParameters.Select(p => p.Clone(innerMethod)));
            innerMethod.ImplAttributes = method.ImplAttributes;
            innerMethod.SemanticsAttributes = method.SemanticsAttributes;
            innerMethod.Body.InitLocals = method.Body.InitLocals;
            innerMethod.Parameters.AddRange(method.Parameters);
            innerMethod.Body.Instructions.AddRange(method.Body.Instructions);
            innerMethod.Body.Variables.AddRange(method.Body.Variables);
            innerMethod.Body.ExceptionHandlers.AddRange(method.Body.ExceptionHandlers);

            // now empty the old one and make it call the inner method...
            method.Body.InitLocals = true;
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
            method.Body.ExceptionHandlers.Clear();
            var instructions = new Instructions(method.Body.Instructions, method.Module);

            var isStatic = method.Attributes.HasFlag(MethodAttributes.Static);
            var firstParameter = isStatic ? 0 : 1;

            // parameters
            var parametersVariable = new VariableDefinition("parameters", moduleDefinition.SafeImport(typeof(object[])));
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
                    instructions.EmitLdarg(parameterIndex + firstParameter); // loads given parameter...
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
            // null or instance
            instructions.Emit(isStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);

            // parameters
            instructions.EmitLdloc(parametersVariable);

            // methods...
            // ...target
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            instructions.Emit(OpCodes.Call, moduleDefinition.SafeImport(ReflectionUtility.GetMethodInfo(() => MethodBase.GetCurrentMethod())));

            // ...inner
            var actionType = moduleDefinition.SafeImport(typeof(Action));
            var actionCtor = moduleDefinition.SafeImport(actionType.Resolve().GetConstructors().Single());

            var delegateType = moduleDefinition.SafeImport(typeof(Delegate));
            var getMethod = moduleDefinition.SafeImport(delegateType.Resolve().Methods.Single(m => m.Name == "get_Method"));

            // ...inner
            instructions.Emit(isStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);
            if (method.IsConstructor)
                instructions.Emit(OpCodes.Castclass, typeof(object));
            instructions.Emit(OpCodes.Ldftn, innerMethod);
            instructions.Emit(OpCodes.Newobj, actionCtor);
            instructions.Emit(OpCodes.Call, getMethod);

            // invoke the method
            var invocationType = TypeResolver.Resolve(moduleDefinition, Binding.InvocationTypeName, true);
            if (invocationType == null)
                return;
            var proceedMethodReference = invocationType.GetMethods().SingleOrDefault(m => m.IsStatic && m.Name == Binding.InvocationProceedAdviceMethodName);
            if (proceedMethodReference == null)
                return;
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
                    instructions.EmitLdarg(parameterIndex + firstParameter); // loads given parameter (it is a ref)
                    instructions.EmitLdloc(parametersVariable); // array
                    instructions.EmitLdc(parameterIndex); // array index
                    instructions.Emit(OpCodes.Ldelem_Ref); // now we have boxed out/ref value
                    instructions.EmitUnboxOrCastIfNecessary(parameter.ParameterType.GetElementType());
                    instructions.EmitStind(parameter.ParameterType); // result is stored in ref parameter
                }
            }

            // and return
            instructions.Emit(OpCodes.Ret);

            lock (method.DeclaringType)
                method.DeclaringType.Methods.Add(innerMethod);
        }

        /// <summary>
        /// Weaves the introductions.
        /// Introduces members as requested by aspects
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="adviceInterface">The advice interface.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        private void WeaveIntroductions(MethodDefinition method, TypeDefinition adviceInterface, ModuleDefinition moduleDefinition)
        {
            var typeDefinition = method.DeclaringType;
            var advices = GetAllMarkers(new MethodReflectionNode(method), adviceInterface);
            var markerAttributeCtor = moduleDefinition.SafeImport(TypeResolver.Resolve(moduleDefinition, Binding.IntroducedFieldAttributeName, true)
                .GetConstructors().Single());
            foreach (var advice in advices)
            {
                var adviceDefinition = advice.Resolve();
                foreach (var field in adviceDefinition.Fields)
                    IntroduceMember(method.Module, field.Name, field.FieldType, field.IsStatic, advice, typeDefinition, markerAttributeCtor);
                foreach (var property in adviceDefinition.Properties)
                    IntroduceMember(method.Module, property.Name, property.PropertyType, !property.HasThis, advice, typeDefinition, markerAttributeCtor);
            }
        }

        /// <summary>
        /// Weaves the information advices.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="infoAdviceInterface">The information advice interface.</param>
        private void WeaveInfoAdvices(ModuleDefinition moduleDefinition, TypeDefinition typeDefinition, TypeDefinition infoAdviceInterface)
        {
            if (GetMarkedMethods(new TypeReflectionNode(typeDefinition), infoAdviceInterface).Any())
            {
                Logger.WriteDebug("Weaving type '{0}' for info", typeDefinition.FullName);
                WeaveInfoAdvices(typeDefinition, moduleDefinition, false);
            }
        }

        /// <summary>
        /// Weaves the method.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="method">The method.</param>
        /// <param name="adviceInterface">The advice interface.</param>
        private void WeaveMethod(ModuleDefinition moduleDefinition, MethodDefinition method, TypeDefinition adviceInterface)
        {
            if (method.HasGenericParameters)
            {
                Logger.WriteWarning("Method {0} has generic parameters, it can not be weaved", method.FullName);
                return;
            }
            WeaveAdvices(method);
            WeaveIntroductions(method, adviceInterface, moduleDefinition);
        }
    }
}
