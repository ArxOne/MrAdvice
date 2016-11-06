#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using System.Collections.Generic;
    using System.Linq;
    using Reflection.Groups;

    /// <summary>
    /// Represents a <see cref="ReflectionNode"/> marked with one or many <see cref="MarkerDefinition"/>
    /// </summary>
    internal class MarkedNode
    {
        /// <summary>
        /// Gets the node it self.
        /// </summary>
        /// <value>
        /// The node.
        /// </value>
        public ReflectionNode Node { get; }

        /// <summary>
        /// Gets the <see cref="MarkerDefinition"/> array applied here.
        /// </summary>
        /// <value>
        /// The definitions.
        /// </value>
        public List<MarkerDefinition> Definitions { get; }

        /// <summary>
        /// Gets a value indicating whether markers at this node will abstract it (remove its execution point)d.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [abstract target]; otherwise, <c>false</c>.
        /// </value>
        public bool AbstractTarget => Definitions.Any(d => d.AbstractTarget);

        public MarkedNode(ReflectionNode node, IEnumerable<MarkerDefinition> definitions)
        {
            Node = node;
            Definitions = definitions.ToList();
        }
    }
}
