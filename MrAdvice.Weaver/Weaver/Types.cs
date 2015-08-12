#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using Mono.Cecil;

    internal class Types
    {
        public TypeDefinition PriorityAttributeType;
        public TypeDefinition AbstractTargetAttributeType;
    }
}
