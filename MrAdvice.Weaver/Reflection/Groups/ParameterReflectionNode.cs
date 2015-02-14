#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using Mono.Cecil;

    /// <summary>
    /// Reflection node, parameter level
    /// </summary>
    internal class ParameterReflectionNode : ReflectionNode
    {
        private readonly ParameterDefinition _parameterDefinition;
        private readonly MethodDefinition _methodDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        public override ReflectionNode Parent
        {
            get { return new MethodReflectionNode(_methodDefinition); }
        }

        private static readonly ReflectionNode[] NoChild = new ReflectionNode[0];

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public override IEnumerable<ReflectionNode> Children
        {
            get { return NoChild; }
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes
        {
            get { return _parameterDefinition.CustomAttributes; }
        }

        private string DebugString { get { return string.Format("Parameter {0}", _parameterDefinition.Name); } }

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
        /// Initializes a new instance of the <see cref="ParameterReflectionNode"/> class.
        /// </summary>
        /// <param name="parameterDefinition">The parameter definition.</param>
        /// <param name="methodDefinition">The method definition.</param>
        public ParameterReflectionNode(ParameterDefinition parameterDefinition, MethodDefinition methodDefinition)
        {
            _parameterDefinition = parameterDefinition;
            _methodDefinition = methodDefinition;
        }
    }
}