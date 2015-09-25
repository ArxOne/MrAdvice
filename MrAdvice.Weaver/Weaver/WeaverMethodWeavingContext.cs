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
    using Advice;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Utility;

    internal class WeaverMethodWeavingContext : MethodWeavingContext
    {
        private readonly TypeDefinition _typeDefinition;
        private readonly Types _types;

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public virtual IEnumerable<string> Properties
        {
            get { return _typeDefinition.Properties.Select(p => p.Name); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeaverMethodWeavingContext" /> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition (type being built).</param>
        /// <param name="type">The type (original type).</param>
        /// <param name="targetMethodName">Name of the target method.</param>
        /// <param name="types">The types.</param>
        public WeaverMethodWeavingContext(TypeDefinition typeDefinition, Type type, string targetMethodName, Types types)
            : base(type, targetMethodName)
        {
            _typeDefinition = typeDefinition;
            _types = types;
        }

        public override bool AddPublicAutoProperty(string name, Type type)
        {
            var moduleDefinition = _typeDefinition.Module;
            var typeReference = moduleDefinition.SafeImport(type);
            // backing field
            var backingFieldDefinition = new FieldDefinition($"<{name}>k__BackingField", FieldAttributes.Private, typeReference);
            backingFieldDefinition.CustomAttributes.Add(CreateCompilerGeneratedAttribute());
            _typeDefinition.Fields.Add(backingFieldDefinition);
            // property...
            var propertyDefinition = new PropertyDefinition(name, PropertyAttributes.None, typeReference);
            _typeDefinition.Properties.Add(propertyDefinition);
            // ...setter
            propertyDefinition.SetMethod = CreatePropertyMethod("set_" + name, moduleDefinition.Import(typeof(void)));
            propertyDefinition.SetMethod.CustomAttributes.Add(CreateCompilerGeneratedAttribute());
            _typeDefinition.Methods.Add(propertyDefinition.SetMethod);
            var setterParameter = new ParameterDefinition("value", ParameterAttributes.In, typeReference);
            propertyDefinition.SetMethod.Parameters.Add(setterParameter);
            var setterIntructions = new Instructions(propertyDefinition.SetMethod.Body.Instructions, moduleDefinition);
            setterIntructions.Emit(OpCodes.Ldarg_0);
            setterIntructions.Emit(OpCodes.Ldarg_1);
            setterIntructions.Emit(OpCodes.Stfld, backingFieldDefinition);
            setterIntructions.Emit(OpCodes.Ret);
            // ...getter
            propertyDefinition.GetMethod = CreatePropertyMethod("get_" + name, typeReference);
            propertyDefinition.GetMethod.CustomAttributes.Add(CreateCompilerGeneratedAttribute());
            _typeDefinition.Methods.Add(propertyDefinition.GetMethod);
            var getterIntructions = new Instructions(propertyDefinition.GetMethod.Body.Instructions, moduleDefinition);
            getterIntructions.Emit(OpCodes.Ldarg_0);
            getterIntructions.Emit(OpCodes.Ldfld, backingFieldDefinition);
            getterIntructions.Emit(OpCodes.Ret);
            return true;
        }

        private MethodDefinition CreatePropertyMethod(string name, TypeReference type)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            var methodDefinition = new MethodDefinition(name, methodAttributes, type);
            methodDefinition.Body = new MethodBody(methodDefinition);
            methodDefinition.Body.InitLocals = true;
            return methodDefinition;
        }

        private CustomAttribute CreateCompilerGeneratedAttribute()
        {
            var constructor = _types.CompilerGeneratedAttributeType.Resolve().GetConstructors().Single();
            return new CustomAttribute(_typeDefinition.Module.SafeImport(constructor), new byte[] { 1, 0, 0, 0 });
        }
    }
}
