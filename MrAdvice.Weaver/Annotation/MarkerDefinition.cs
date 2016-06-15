#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using dnlib.DotNet;

    internal class MarkerDefinition
    {
        public ITypeDefOrRef Type;

        public int Priority;

        public bool AbstractTarget;
    }
}
