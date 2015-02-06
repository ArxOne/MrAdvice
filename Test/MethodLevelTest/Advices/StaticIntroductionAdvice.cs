#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest.Advices
{
    using System;
    using ArxOne.Weavisor.Advice;
    using ArxOne.Weavisor.Introduction;

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
