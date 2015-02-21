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

    public class StaticIntroductionAdvice : Attribute, IMethodAdvice
    {
        [ThreadStatic]
        public static int LastStaticAdvicesCount;

        public static IntroducedField<int> StaticAdvicesCount { get; set; }

        public void Advise(MethodAdviceContext context)
        {
            LastStaticAdvicesCount = ++StaticAdvicesCount[context];
            context.Proceed();
        }
    }
}
