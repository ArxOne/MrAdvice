﻿#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using System.Linq;
    using dnlib.DotNet;

    /// <summary>
    /// Reflection group, type level
    /// </summary>
    internal class TypeReflectionNode : ReflectionNode
    {
        /// <summary>
        /// Gets the type definition.
        /// </summary>
        /// <value>
        /// The type definition.
        /// </value>
        public override TypeDef Type { get; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent() => new ModuleReflectionNode(Type.Module, null);

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        protected override IEnumerable<ReflectionNode> LoadChildren()
        {
            foreach (var eventDefinition in Type.Events.OrderBy(p => p.Name))
                yield return new EventReflectionNode(eventDefinition, this);
            foreach (var propertyDefinition in Type.Properties.OrderBy(p => p.Name))
                yield return new PropertyReflectionNode(propertyDefinition, this);
            foreach (var constructorMethodDefinition in Type.FindConstructors())
                yield return new MethodReflectionNode(constructorMethodDefinition, this);
            foreach (var methodDefinition in Type.Methods.OrderBy(m => m.Name).Where(m => !m.IsSpecialName))
                yield return new MethodReflectionNode(methodDefinition, this);
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes => Type.CustomAttributes;

        /// <summary>
        /// Gets a value indicating whether this instance is generic.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is generic; otherwise, <c>false</c>.
        /// </value>
        public override bool IsGeneric => Type.HasGenericParameters;

        public override string Name => Type.FullName;

        private string DebugString => $"Type {Type.FullName}";

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return DebugString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReflectionNode" /> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="parent">The parent.</param>
        public TypeReflectionNode(TypeDef typeDefinition, ReflectionNode parent)
        {
            Type = typeDefinition;
            Parent = parent;
        }
    }
}