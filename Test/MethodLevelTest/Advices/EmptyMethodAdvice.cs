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

    public class EmptyMethodAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }

    public class EmptyMethodAdvice2 : EmptyMethodAdvice
    {
    }
}