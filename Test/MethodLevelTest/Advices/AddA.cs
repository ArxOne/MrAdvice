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
    using ArxOne.MrAdvice.Annotation;

    [Priority(10)]
    public class AddA: Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Parameters[0] = (string) context.Parameters[0] + "A";
            context.Proceed();
        }
    }

    [Priority(9)]
    public class AddB: Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Parameters[0] = (string) context.Parameters[0] + "B";
            context.Proceed();
        }
    }

    [Priority(8)]
    public class AddC: Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Parameters[0] = (string) context.Parameters[0] + "C";
            context.Proceed();
        }
    }

    [Priority(7)]
    public class AddD: Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Parameters[0] = (string) context.Parameters[0] + "D";
            context.Proceed();
        }
    }

    [Priority(6)]
    public class AddE: Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Parameters[0] = (string) context.Parameters[0] + "E";
            context.Proceed();
        }
    }
}
