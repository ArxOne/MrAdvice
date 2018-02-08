#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using dnlib.DotNet;
    using Utility;

    /// <summary>
    /// Reflection group, assembly level
    /// </summary>
    internal class AssemblyReflectionNode : ReflectionNode
    {
        private readonly AssemblyDef _assemblyDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent() => null;

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        protected override IEnumerable<ReflectionNode> LoadChildren() => new[] { new ModuleReflectionNode(_assemblyDefinition.GetMainModule(), this) };

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes => _assemblyDefinition.CustomAttributes;

        public override string Name => _assemblyDefinition.FullName;

        private string DebugString => $"Assembly {_assemblyDefinition.FullName}";

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => DebugString;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyReflectionNode"/> class.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly definition.</param>
        public AssemblyReflectionNode(AssemblyDef assemblyDefinition)
        {
            _assemblyDefinition = assemblyDefinition;
        }
    }
}