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
    /// Reflection group, assembly level
    /// </summary>
    internal class AssemblyReflectionNode : ReflectionNode
    {
        private readonly AssemblyDefinition _assemblyDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        public override ReflectionNode Parent
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public override IEnumerable<ReflectionNode> Children
        {
            get { return new[] { new ModuleReflectionNode(_assemblyDefinition.MainModule) }; }
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes
        {
            get { return _assemblyDefinition.CustomAttributes; }
        }

        private string DebugString { get { return string.Format("Assembly {0}", _assemblyDefinition.FullName); } }

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
        /// Initializes a new instance of the <see cref="AssemblyReflectionNode"/> class.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly definition.</param>
        public AssemblyReflectionNode(AssemblyDefinition assemblyDefinition)
        {
            _assemblyDefinition = assemblyDefinition;
        }
    }
}