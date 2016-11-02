#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest.Advices
{
    using System;
    using System.Collections.Generic;
    using ArxOne.MrAdvice.Advice;
    using ArxOne.MrAdvice.Introduction;
    using ExternalAdviceTest;

    public class ComplexIntroductionAdvice : Attribute, IMethodAdvice
    {
        public IntroducedField<IList<ExternalClass>> ALists { get; set; }

        public void Advise(MethodAdviceContext context)
        {
            if (ALists[context] == null)
                ALists[context] = new List<ExternalClass>();
            ALists[context].Add(new ExternalClass());
            var c = ALists[context].Count;
            context.Proceed();
        }
    }
}