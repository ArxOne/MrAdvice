#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using AssemblyLevelTest;

[assembly: SelfExcludingAdvice]

namespace AssemblyLevelTest
{
    using System;
    using ArxOne.MrAdvice.Advice;

    public class SelfExcludingAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }
}
