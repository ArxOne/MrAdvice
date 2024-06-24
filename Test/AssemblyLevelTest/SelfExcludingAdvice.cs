#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using AssemblyLevelTest;

[assembly: SelfExcludingAdvice(1)]

namespace AssemblyLevelTest
{
    using System;
    using ArxOne.MrAdvice.Advice;

    public class SelfExcludingAdvice : Attribute, IMethodAdvice
    {
        public int increment;
        public static int counter = 0;
        public SelfExcludingAdvice(int increment) { this.increment = increment; }

        public void Advise(MethodAdviceContext context)
        {
            counter += increment;
            context.Proceed();
        }
    }
}
