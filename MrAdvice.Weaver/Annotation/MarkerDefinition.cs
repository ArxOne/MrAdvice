#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using dnlib.DotNet;
    using Reflection.Groups;

    /// <summary>
    /// A marker is something applied to a <see cref="ReflectionNode"/>
    /// Several markers may be applied to a node using the <see cref="MarkedNode"/> class
    /// </summary>
    internal class MarkerDefinition
    {
        /// <summary>
        /// Gets the type of the marker.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public TypeDef Type { get; }

        /// <summary>
        /// Gets a value indicating whether the marker abstracts target.
        /// This instructs the weaver to remove execution point.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [abstract target]; otherwise, <c>false</c>.
        /// </value>
        public bool AbstractTarget { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerDefinition"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="abstractTarget">if set to <c>true</c> [abstract target].</param>
        public MarkerDefinition(TypeDef type, bool abstractTarget)
        {
            Type = type;
            AbstractTarget = abstractTarget;
        }
    }
}
