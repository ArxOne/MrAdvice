#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    /// <summary>
    /// Reflection group, module level
    /// </summary>
    internal class ModuleReflectionNode : ReflectionNode
    {
        private readonly ModuleDefinition _moduleDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        public override ReflectionNode Parent
        {
            get { return new AssemblyReflectionNode(_moduleDefinition.Assembly); }
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public override IEnumerable<ReflectionNode> Children
        {
            get { return _moduleDefinition.GetTypes().OrderBy(t => t.Name).Select(t => new TypeReflectionNode(t)); }
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes
        {
            get { return _moduleDefinition.CustomAttributes; }
        }

        private string DebugString { get { return string.Format("Module {0}", _moduleDefinition.FullyQualifiedName); } }

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
        /// Initializes a new instance of the <see cref="ModuleReflectionNode"/> class.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        public ModuleReflectionNode(ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition;
        }
    }
}