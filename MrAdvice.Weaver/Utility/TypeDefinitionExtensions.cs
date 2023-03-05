#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Xml;

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
        /// Gets the self and ancestors, from closest to farthest.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="typeResolver">The type resolver.</param>
        /// <returns></returns>
        public static IEnumerable<TypeDef> GetSelfAndAncestors(this TypeDef typeDefinition, TypeResolver typeResolver = null)
        {
            while (typeDefinition is not null)
            {
                yield return typeDefinition;
                var baseType = typeDefinition.BaseType;
                if (baseType is null)
                    break;
                typeDefinition = SafeResolve(typeResolver, baseType);
            }
        }

        /// <summary>
        /// Gets the ancestors, from closest to farthest.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="typeResolver">The type resolver.</param>
        /// <returns></returns>
        public static IEnumerable<TypeDef> GetAncestors(this TypeDef typeDefinition, TypeResolver typeResolver = null)
        {
            return GetSelfAndAncestors(typeDefinition).Skip(1);
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
            if (typeResolver is null)
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
        /// <param name="methodAttributes">The method attributes.</param>
        internal static void AddAutoProperty(this TypeDef typeDefinition, string name, ITypeDefOrRef typeReference, ModuleDef moduleDefinition, TypeResolver typeResolver,
            System.Reflection.MethodAttributes methodAttributes)
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
            propertyDefinition.SetMethod = CreatePropertyMethod("set_" + name, moduleDefinition.CorLibTypes.Void, methodAttributes, Tuple.Create(typeReference.ToTypeSig(), "value"));
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
            propertyDefinition.GetMethod = CreatePropertyMethod("get_" + name, typeReference.ToTypeSig(), methodAttributes);
            propertyDefinition.GetMethod.CustomAttributes.Add(moduleDefinition.CreateCustomAttribute(compilerGeneratedAttribute, typeResolver));
            typeDefinition.Methods.Add(propertyDefinition.GetMethod);
            var getterInstructions = new Instructions(propertyDefinition.GetMethod.Body, moduleDefinition);
            getterInstructions.Emit(OpCodes.Ldarg_0);
            getterInstructions.Emit(OpCodes.Ldfld, backingFieldDefinition);
            getterInstructions.Emit(OpCodes.Ret);
        }

        private static MethodAttributes ToMethodAttributes(this System.Reflection.MethodAttributes attributes)
        {
            return (MethodAttributes)(attributes & System.Reflection.MethodAttributes.MemberAccessMask);
        }

        /// <summary>
        /// Creates a property method (getter or setter).
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="attributes"></param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static MethodDefUser CreatePropertyMethod(string name, TypeSig returnType,
            System.Reflection.MethodAttributes attributes, params Tuple<TypeSig, string>[] parameters)
        {
            var methodAttributes = ToMethodAttributes(attributes) | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
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
            foreach (var ancestorType in typeDefinition.GetSelfAndAncestors(typeResolver))
            {
                if (ancestorType.SafeEquivalent(parent))
                    return true;
            }

            return false;
        }

        public static MethodDef GetOrCreateMethod(this TypeDef typeDefinition, string methodName, MethodSig methodSig, MethodAttributes attributes)
        {
            return GetOrCreateAnyMethod(typeDefinition, methodName, methodSig, attributes).Item1;
        }

        private static Tuple<MethodDef, bool> GetOrCreateAnyMethod(this TypeDef typeDefinition, string methodName, MethodSig methodSig, MethodAttributes attributes)
        {
            var existingMethod = typeDefinition.FindMethod(methodName, methodSig);
            if (existingMethod is not null)
                return Tuple.Create(existingMethod, false);
            var newMethod = new MethodDefUser(methodName, methodSig)
            {
                Body = new CilBody(),
                Attributes = attributes
            };
            newMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
            typeDefinition.Methods.Add(newMethod);
            return Tuple.Create<MethodDef, bool>(newMethod, true);
        }

        public static MethodDef FindMethodCheckBaseType(this TypeDef typeDefinition, string methodName, MethodSig methodSig, TypeResolver typeResolver)
        {
            foreach (var selfAndParent in typeDefinition.GetSelfAndAncestors(typeResolver))
            {
                var methodDef = methodSig is null
                    ? selfAndParent.FindMethod(methodName)
                    : selfAndParent.FindMethod(methodName, methodSig);
                if (methodDef is not null)
                    return methodDef;
            }
            return null;
        }

        public static MethodDef GetOrCreateFinalizer(this TypeDef typeDefinition, TypeResolver typeResolver)
        {
            const string finalizerName = "Finalize";
            var finalizerSig = new MethodSig(CallingConvention.HasThis, 0, typeDefinition.Module.CorLibTypes.Void, Array.Empty<TypeSig>());
            const MethodAttributes attributes = MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            var existingMethod = typeDefinition.FindMethod(finalizerName, finalizerSig);
            if (existingMethod is not null)
                return existingMethod;

            var baseFinalizer = typeDefinition.Module.SafeImport(typeDefinition.FindMethodCheckBaseType(finalizerName, finalizerSig, typeResolver));

            var newMethod = new MethodDefUser(finalizerName, finalizerSig)
            {
                Body = new CilBody { InitLocals = true },
                Attributes = attributes
            };
            var instructions = new Instructions(newMethod.Body, typeDefinition.Module);
            var finalRet = Instruction.Create(OpCodes.Ret);
            instructions.Emit(OpCodes.Nop).KeepLast(out var tryFirst);
            instructions.Emit(OpCodes.Leave, finalRet);
            instructions.Emit(OpCodes.Ldarg_0).KeepLast(out var finallyFirst);
            if (baseFinalizer is not null)
                instructions.Emit(OpCodes.Call, baseFinalizer);
            instructions.Emit(OpCodes.Endfinally);
            instructions.Emit(finalRet);
            newMethod.Body.ExceptionHandlers.Add(new ExceptionHandler
            {
                HandlerType = ExceptionHandlerType.Finally,
                TryStart = tryFirst,
                TryEnd = finallyFirst,
                HandlerStart = finallyFirst,
                HandlerEnd = finalRet
            });

            typeDefinition.Methods.Add(newMethod);
            return newMethod;
        }
    }
}
