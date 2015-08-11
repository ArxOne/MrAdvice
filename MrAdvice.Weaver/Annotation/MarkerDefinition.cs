#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using Mono.Cecil;

    internal class MarkerDefinition
    {
        public TypeReference Type;

        public int Priority;
    }
}
