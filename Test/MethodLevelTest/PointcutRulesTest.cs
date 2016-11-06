#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
    using ArxOne.MrAdvice.Advice;
    using ArxOne.MrAdvice.Annotation;

    public class PointcutRulesTest
    {
        [IncludePointcut("*.Set@")]
        public class SetterAdvice : Attribute, IMethodAdvice
        {
            public void Advise(MethodAdviceContext context)
            {
                context.Proceed();
            }
        }

        [SetterAdvice]
        public class SetterAdvisedType
        {
            public void SetA() { }
            public void ZetA() { }
            public void SetB() { }
        }
    }
}
