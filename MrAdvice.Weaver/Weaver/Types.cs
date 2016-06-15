#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using dnlib.DotNet;

    /// <summary>
    /// Common markers
    /// </summary>
    internal class Types
    {
        public ITypeDefOrRef CompilerGeneratedAttributeType;
        public ITypeDefOrRef PriorityAttributeType;
        public ITypeDefOrRef AbstractTargetAttributeType;

        public ITypeDefOrRef WeavingAdviceAttributeType;
    }
}
