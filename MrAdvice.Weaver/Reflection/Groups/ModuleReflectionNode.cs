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

    /// <summary>
    /// Reflection group, module level
    /// </summary>
    internal class ModuleReflectionNode : ReflectionNode
    {
        private readonly ModuleDef _moduleDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent() => new AssemblyReflectionNode(_moduleDefinition.Assembly);

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        protected override IEnumerable<ReflectionNode> LoadChildren() => _moduleDefinition.GetTypes().OrderBy(t => t.Name).Select(t => new TypeReflectionNode(t, this));

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes => _moduleDefinition.CustomAttributes;

        public override string Name => _moduleDefinition.Name;

        private string DebugString => $"Module {_moduleDefinition.FullName}";

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => DebugString;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleReflectionNode" /> class.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="parent">The parent.</param>
        public ModuleReflectionNode(ModuleDef moduleDefinition, AssemblyReflectionNode parent)
        {
            _moduleDefinition = moduleDefinition;
            Parent = parent;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ModuleDef"/> to <see cref="ModuleReflectionNode"/>.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ModuleReflectionNode(ModuleDef moduleDefinition) => new ModuleReflectionNode(moduleDefinition, null);
    }
}