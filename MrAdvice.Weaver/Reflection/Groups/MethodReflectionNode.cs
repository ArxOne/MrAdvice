#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Utility;

    /// <summary>
    /// Reflection node, method level
    /// </summary>
    internal class MethodReflectionNode : ReflectionNode
    {
        private readonly MethodDefinition _methodDefinition;
        private PropertyDefinition _propertyDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent()
        {
            if (_propertyDefinition != null)
                return new PropertyReflectionNode(_propertyDefinition);
            // a bit tricky here, since a method can belong to a property
            var declaringType = _methodDefinition.DeclaringType;
            if (_methodDefinition.IsPropertyMethod())
            {
                var propertyName = ReflectionUtility.GetPropertyName(_methodDefinition.Name);
                _propertyDefinition = declaringType.Properties.Single(p => p.Name == propertyName);
                return new PropertyReflectionNode(_propertyDefinition);
            }
            return new TypeReflectionNode(declaringType);
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override IEnumerable<ReflectionNode> LoadChildren()
        {
            return _methodDefinition.Parameters.Select(p => new ParameterReflectionNode(p, _methodDefinition));
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
                return _methodDefinition.CustomAttributes
                    .Concat(_methodDefinition.MethodReturnType.CustomAttributes); // return type has attributes
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is generic.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is generic; otherwise, <c>false</c>.
        /// </value>
        public override bool IsGeneric
        {
            get { return _methodDefinition.HasGenericParameters; }
        }

        /// <summary>
        /// Gets the method matching this node.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public override MethodDefinition Method { get { return _methodDefinition; } }

        private string DebugString { get { return string.Format("Method {0}", _methodDefinition.FullName); } }

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
        /// Initializes a new instance of the <see cref="MethodReflectionNode" /> class.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <param name="propertyDefinition">The property.</param>
        public MethodReflectionNode(MethodDefinition methodDefinition, PropertyDefinition propertyDefinition = null)
        {
            _methodDefinition = methodDefinition;
            _propertyDefinition = propertyDefinition;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="MethodDefinition"/> to <see cref="MethodReflectionNode"/>.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MethodReflectionNode(MethodDefinition methodDefinition)
        {
            return new MethodReflectionNode(methodDefinition);
        }
    }
}