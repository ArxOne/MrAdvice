#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using Weaver;
    using FieldAttributes = dnlib.DotNet.FieldAttributes;
    using MethodAttributes = dnlib.DotNet.MethodAttributes;

    public static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Gets the self and parents.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="typeResolver">The type resolver.</param>
        /// <returns></returns>
        public static IEnumerable<TypeDef> GetSelfAndParents(this TypeDef typeDefinition, TypeResolver typeResolver = null)
        {
            while (typeDefinition != null)
            {
                yield return typeDefinition;
                var baseType = typeDefinition.BaseType;
                if (baseType == null)
                    break;
                typeDefinition = SafeResolve(typeResolver, baseType);
            }
        }

        /// <summary>
        /// Always resolves the type, whether there is a <see cref="TypeResolver"/> or not.
        /// </summary>
        /// <param name="typeResolver">The type resolver.</param>
        /// <param name="typeDefOrRef">Type of the base.</param>
        /// <returns></returns>
        public static TypeDef SafeResolve(this TypeResolver typeResolver, ITypeDefOrRef typeDefOrRef)
        {
            TypeDef typeDefinition;
            if (typeResolver == null)
                typeDefinition = typeDefOrRef.ResolveTypeDef();
            else
                typeDefinition = typeResolver.Resolve(typeDefOrRef);
            return typeDefinition;
        }

        public static IEnumerable<TypeDef> GetAllInterfaces(this TypeDef typeDefinition, TypeResolver typeResolver = null)
        {
            return GetAllInterfacesRaw(typeDefinition, typeResolver).Distinct();
        }

        private static IEnumerable<TypeDef> GetAllInterfacesRaw(this TypeDef typeDefinition, TypeResolver typeResolver = null)
        {
            var allInterfaces = new List<TypeDef>();
            if (typeDefinition.IsInterface)
                allInterfaces.Add(typeDefinition);
            foreach (var parentInterfaceImpl in typeDefinition.Interfaces)
            {
                var @interface = typeResolver.SafeResolve(parentInterfaceImpl.Interface);
                allInterfaces.AddRange(GetAllInterfacesRaw(@interface, typeResolver));
            }

            return allInterfaces;
        }

        /// <summary>
        /// Adds a public automatic property.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="name">The name.</param>
        /// <param name="typeReference">The type reference.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="typeResolver">The type resolver.</param>
        internal static void AddPublicAutoProperty(this TypeDef typeDefinition, string name, ITypeDefOrRef typeReference, ModuleDef moduleDefinition, TypeResolver typeResolver)
        {
            var compilerGeneratedAttribute = moduleDefinition.SafeImport(typeof(CompilerGeneratedAttribute));
            // backing field
            var backingFieldDefinition = new FieldDefUser($"<{name}>k__BackingField", new FieldSig(typeReference.ToTypeSig()), FieldAttributes.Private);
            backingFieldDefinition.CustomAttributes.Add(moduleDefinition.CreateCustomAttribute(compilerGeneratedAttribute, typeResolver));
            typeDefinition.Fields.Add(backingFieldDefinition);
            // property...
            var propertyDefinition = new PropertyDefUser(name, new PropertySig(true, typeReference.ToTypeSig()));
            typeDefinition.Properties.Add(propertyDefinition);
            // ...setter
            propertyDefinition.SetMethod = CreatePropertyMethod("set_" + name, moduleDefinition.CorLibTypes.Void, Tuple.Create(typeReference.ToTypeSig(), "value"));
            propertyDefinition.SetMethod.CustomAttributes.Add(moduleDefinition.CreateCustomAttribute(compilerGeneratedAttribute, typeResolver));
            typeDefinition.Methods.Add(propertyDefinition.SetMethod);
            var setterParameter = new ParamDefUser("value");
            propertyDefinition.SetMethod.ParamDefs.Add(setterParameter);
            var setterInstructions = new Instructions(propertyDefinition.SetMethod.Body, moduleDefinition);
            setterInstructions.Emit(OpCodes.Ldarg_0);
            setterInstructions.Emit(OpCodes.Ldarg_1);
            setterInstructions.Emit(OpCodes.Stfld, backingFieldDefinition);
            setterInstructions.Emit(OpCodes.Ret);
            // ...getter
            propertyDefinition.GetMethod = CreatePropertyMethod("get_" + name, typeReference.ToTypeSig());
            propertyDefinition.GetMethod.CustomAttributes.Add(moduleDefinition.CreateCustomAttribute(compilerGeneratedAttribute, typeResolver));
            typeDefinition.Methods.Add(propertyDefinition.GetMethod);
            var getterInstructions = new Instructions(propertyDefinition.GetMethod.Body, moduleDefinition);
            getterInstructions.Emit(OpCodes.Ldarg_0);
            getterInstructions.Emit(OpCodes.Ldfld, backingFieldDefinition);
            getterInstructions.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Creates a property method (getter or setter).
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static MethodDefUser CreatePropertyMethod(string name, TypeSig returnType, params Tuple<TypeSig, string>[] parameters)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            var methodSig = new MethodSig(CallingConvention.HasThis, 0, returnType, parameters.Select(p => p.Item1).ToArray());
            var methodDefinition = new MethodDefUser(name, methodSig, methodAttributes);
            var methodParameters = new MethodParameters(methodDefinition);
            for (int index = 0; index < parameters.Length; index++)
                methodParameters[0].ParamDef.Name = parameters[index].Item2;
            methodDefinition.Body = new CilBody { InitLocals = true };
            return methodDefinition;
        }

        /// <summary>
        /// Indicates whether the given type implements another (or is the other).
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="typeResolver">The type resolver.</param>
        /// <returns></returns>
        public static bool ImplementsType(this TypeDef typeDefinition, ITypeDefOrRef parent, TypeResolver typeResolver = null)
        {
            foreach (var ancestorType in typeDefinition.GetSelfAndParents(typeResolver))
            {
                if (ancestorType.SafeEquivalent(parent))
                    return true;
            }

            return false;
        }
    }
}
