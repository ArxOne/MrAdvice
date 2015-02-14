#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using IO;
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
        public override ReflectionNode Parent
        {
            get { return new TypeReflectionNode(_propertyDefinition.DeclaringType); }
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override IEnumerable<ReflectionNode> Children
        {
            get
            {
                if (_propertyDefinition.GetMethod != null)
                    yield return new MethodReflectionNode(_propertyDefinition.GetMethod, _propertyDefinition);
                if (_propertyDefinition.SetMethod != null)
                    yield return new MethodReflectionNode(_propertyDefinition.SetMethod, _propertyDefinition);
            }
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes
        {
            get
            {
                //Logger.WriteDebug("> {0}", _propertyDefinition.FullName);
                //foreach (var a in _propertyDefinition.CustomAttributes)
                //    Logger.WriteDebug(">> {0}", a.AttributeType.FullName);
                return _propertyDefinition.CustomAttributes;
            }
        }

        private string DebugString { get { return string.Format("Property {0}", _propertyDefinition.FullName); } }

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
        /// Initializes a new instance of the <see cref="PropertyReflectionNode"/> class.
        /// </summary>
        /// <param name="propertyDefinition">The property definition.</param>
        public PropertyReflectionNode(PropertyDefinition propertyDefinition)
        {
            _propertyDefinition = propertyDefinition;
        }
    }
}