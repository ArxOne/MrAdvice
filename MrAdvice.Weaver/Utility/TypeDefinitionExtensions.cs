#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Weaver;

    public static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Gets the self and parents.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <returns></returns>
        public static IEnumerable<TypeDefinition> GetSelfAndParents(this TypeDefinition typeDefinition)
        {
            for (;;)
            {
                yield return typeDefinition;
                var baseType = typeDefinition.BaseType;
                if (baseType == null)
                    break;
                typeDefinition = baseType.Resolve();
            }
        }

        /// <summary>
        /// Adds a public automatic property.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        public static void AddPublicAutoProperty(this TypeDefinition typeDefinition, string name, TypeReference type)
        {
            var moduleDefinition = typeDefinition.Module;
            var typeReference = moduleDefinition.SafeImport(type);
            var compilerGeneratedAttribute = moduleDefinition.SafeImport(typeof(CompilerGeneratedAttribute));
            // backing field
            var backingFieldDefinition = new FieldDefinition($"<{name}>k__BackingField", FieldAttributes.Private, typeReference);
            backingFieldDefinition.CustomAttributes.Add(moduleDefinition.CreateCustomAttribute(compilerGeneratedAttribute));
            typeDefinition.Fields.Add(backingFieldDefinition);
            // property...
            var propertyDefinition = new PropertyDefinition(name, PropertyAttributes.None, typeReference);
            typeDefinition.Properties.Add(propertyDefinition);
            // ...setter
            propertyDefinition.SetMethod = CreatePropertyMethod("set_" + name, moduleDefinition.Import(typeof(void)));
            propertyDefinition.SetMethod.CustomAttributes.Add(moduleDefinition.CreateCustomAttribute(compilerGeneratedAttribute));
            typeDefinition.Methods.Add(propertyDefinition.SetMethod);
            var setterParameter = new ParameterDefinition("value", ParameterAttributes.In, typeReference);
            propertyDefinition.SetMethod.Parameters.Add(setterParameter);
            var setterIntructions = new Instructions(propertyDefinition.SetMethod.Body.Instructions, moduleDefinition);
            setterIntructions.Emit(OpCodes.Ldarg_0);
            setterIntructions.Emit(OpCodes.Ldarg_1);
            setterIntructions.Emit(OpCodes.Stfld, backingFieldDefinition);
            setterIntructions.Emit(OpCodes.Ret);
            // ...getter
            propertyDefinition.GetMethod = CreatePropertyMethod("get_" + name, typeReference);
            propertyDefinition.GetMethod.CustomAttributes.Add(moduleDefinition.CreateCustomAttribute(compilerGeneratedAttribute));
            typeDefinition.Methods.Add(propertyDefinition.GetMethod);
            var getterIntructions = new Instructions(propertyDefinition.GetMethod.Body.Instructions, moduleDefinition);
            getterIntructions.Emit(OpCodes.Ldarg_0);
            getterIntructions.Emit(OpCodes.Ldfld, backingFieldDefinition);
            getterIntructions.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Creates a property method (getter or setter).
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static MethodDefinition CreatePropertyMethod(string name, TypeReference type)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            var methodDefinition = new MethodDefinition(name, methodAttributes, type);
            methodDefinition.Body = new MethodBody(methodDefinition) { InitLocals = true };
            return methodDefinition;
        }


    }
}