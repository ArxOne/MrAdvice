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
    using dnlib.DotNet;
    using Utility;

    /// <summary>
    /// Reflection node, method level
    /// </summary>
    internal class MethodReflectionNode : ReflectionNode
    {
        private readonly MethodDef _methodDefinition;
        private PropertyDef _propertyDefinition;
        private EventDef _eventDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent()
        {
            if (_propertyDefinition != null)
                return new PropertyReflectionNode(_propertyDefinition, null);
            if (_eventDefinition != null)
                return new EventReflectionNode(_eventDefinition, null);
            // a bit tricky here, since a method can belong to a property
            var declaringType = _methodDefinition.DeclaringType;
            if (_methodDefinition.IsPropertyMethod())
            {
                _propertyDefinition = declaringType.Properties.Single(p => p.GetMethod == _methodDefinition || p.SetMethod == _methodDefinition);
                return new PropertyReflectionNode(_propertyDefinition, null);
            }
            if (_methodDefinition.IsEventMethod())
            {
                _eventDefinition = declaringType.Events.Single(p => p.AddMethod == _methodDefinition || p.RemoveMethod == _methodDefinition);
                return new EventReflectionNode(_eventDefinition, null);
            }
            return new TypeReflectionNode(declaringType, null);
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        protected override IEnumerable<ReflectionNode> LoadChildren() => _methodDefinition.Parameters.Where(p => !p.IsHiddenThisParameter && p.ParamDef != null)
            .Select(p => new ParameterReflectionNode(p.ParamDef, _methodDefinition, this));

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
                IEnumerable<CustomAttribute> customAttributes = _methodDefinition.CustomAttributes;
                if (_methodDefinition.Parameters.ReturnParameter.ParamDef != null)
                    customAttributes = customAttributes.Concat(_methodDefinition.Parameters.ReturnParameter.ParamDef.CustomAttributes);
                return customAttributes;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is generic.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is generic; otherwise, <c>false</c>.
        /// </value>
        public override bool IsGeneric => _methodDefinition.HasGenericParameters;

        /// <summary>
        /// Gets the method matching this node.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public override MethodDef Method => _methodDefinition;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => _methodDefinition.Name;

        private string DebugString => $"Method {_methodDefinition.FullName}";

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => DebugString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodReflectionNode" /> class.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyDefinition">The property.</param>
        public MethodReflectionNode(MethodDef methodDefinition, ReflectionNode parent, PropertyDef propertyDefinition = null)
        {
            _methodDefinition = methodDefinition;
            _propertyDefinition = propertyDefinition;
            Parent = parent;
        }

        public MethodReflectionNode(MethodDef methodDefinition, ReflectionNode parent, EventDef eventDefinition)
        {
            _methodDefinition = methodDefinition;
            _eventDefinition = eventDefinition;
            Parent = parent;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="MethodDef"/> to <see cref="MethodReflectionNode"/>.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator MethodReflectionNode(MethodDef methodDefinition) => new MethodReflectionNode(methodDefinition, null);
    }
}