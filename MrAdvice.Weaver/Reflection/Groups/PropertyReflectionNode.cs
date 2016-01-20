#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using Mono.Cecil;

    /// <summary>
    /// Reflection group, property level
    /// </summary>
    internal class PropertyReflectionNode : ReflectionNode
    {
        private readonly PropertyDefinition _propertyDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent() => new TypeReflectionNode(_propertyDefinition.DeclaringType);

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        protected override IEnumerable<ReflectionNode> LoadChildren()
        {
            if (_propertyDefinition.GetMethod != null)
                yield return new MethodReflectionNode(_propertyDefinition.GetMethod, _propertyDefinition);
            if (_propertyDefinition.SetMethod != null)
                yield return new MethodReflectionNode(_propertyDefinition.SetMethod, _propertyDefinition);
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes => _propertyDefinition.CustomAttributes;

        private string DebugString => $"Property {_propertyDefinition.FullName}";

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => DebugString;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyReflectionNode"/> class.
        /// </summary>
        /// <param name="propertyDefinition">The property definition.</param>
        public PropertyReflectionNode(PropertyDefinition propertyDefinition)
        {
            _propertyDefinition = propertyDefinition;
        }
    }
}