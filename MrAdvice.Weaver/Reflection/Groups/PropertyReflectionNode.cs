#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using Annotation;
    using dnlib.DotNet;

    /// <summary>
    /// Reflection group, property level
    /// </summary>
    internal class PropertyReflectionNode : ReflectionNode
    {
        private readonly PropertyDef _propertyDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent() => new TypeReflectionNode(_propertyDefinition.DeclaringType, null);

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        protected override IEnumerable<ReflectionNode> LoadChildren()
        {
            if (_propertyDefinition.GetMethod != null)
                yield return new MethodReflectionNode(_propertyDefinition.GetMethod, this, _propertyDefinition);
            if (_propertyDefinition.SetMethod != null)
                yield return new MethodReflectionNode(_propertyDefinition.SetMethod, this, _propertyDefinition);
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes => _propertyDefinition.CustomAttributes;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => _propertyDefinition.Name;

        private string DebugString => $"Property {_propertyDefinition.FullName}";

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => DebugString;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyReflectionNode" /> class.
        /// </summary>
        /// <param name="propertyDefinition">The property definition.</param>
        /// <param name="parent">The parent.</param>
        public PropertyReflectionNode(PropertyDef propertyDefinition, TypeReflectionNode parent)
        {
            _propertyDefinition = propertyDefinition;
            Parent = parent;
        }
    }
}