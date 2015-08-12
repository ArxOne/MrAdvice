#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using System.Linq;
    using Reflection.Groups;

    internal class MarkedNode
    {
        public ReflectionNode Node;
        public MarkerDefinition[] Definitions;
        public bool AbstractTarget => Definitions.Any(d => d.AbstractTarget);
    }
}
