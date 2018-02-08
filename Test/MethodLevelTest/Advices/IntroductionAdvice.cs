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

    public class IntroductionAdvice : Attribute, IMethodAdvice
    {
        [ThreadStatic]
        public static int LastAdvicesCount;

        [ThreadStatic]
        public static string LastRandomString;

        // ReSharper disable once UnassignedField.Global
        [NonSerialized]
        public IntroducedField<int> AdvicesCount;
        public IntroducedField<string> RandomString { get; set; }

        public void Advise(MethodAdviceContext context)
        {
            LastAdvicesCount = ++AdvicesCount[context];
            if (RandomString[context] == null)
                RandomString[context] = "1";
            else
                RandomString[context] += "0";
            LastRandomString = RandomString[context];
            context.Proceed();
        }
    }
}
