#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System.Collections;
    using System.Collections.Generic;
    using dnlib.DotNet;
    using Utility;

    /// <summary>
    /// Weaving context
    /// Allows to gather all common data
    /// </summary>
    public class WeavingContext
    {
        public ITypeDefOrRef CompilerGeneratedAttributeType { get; set; }
        public ITypeDefOrRef PriorityAttributeType { get; set; }
        public ITypeDefOrRef AbstractTargetAttributeType { get; set; }

        public ITypeDefOrRef WeavingAdviceAttributeType { get; set; }

        public TypeDef ShortcutClass { get; set; }

        public IDictionary<bool[], IMethod> ShortcutMethods { get; } = new Dictionary<bool[], IMethod>(new SequenceEqualityComparer<bool>());

        public IMethod InvocationProceedMethod { get; set; }
    }
}
