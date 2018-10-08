#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest.Advices
{
    using System;
    using ArxOne.MrAdvice.Advice;
    using ArxOne.MrAdvice.Introduction;

    public class SharedIntroductionAdvice : Attribute, IMethodAdvice
    {
        [ThreadStatic]
        public static int LastSharedAdvicesCount;

        public SharedIntroducedField<int> SharedAdvicesCount;

        public void Advise(MethodAdviceContext context)
        {
            LastSharedAdvicesCount = ++SharedAdvicesCount[context];
            context.Proceed();
        }
    }
}