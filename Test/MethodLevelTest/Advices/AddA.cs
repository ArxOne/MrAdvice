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
    public class AddA : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Arguments[0] = (string)context.Arguments[0] + "A";
            context.Proceed();
        }
    }

    public class AddA2 : AddA
    { }

    [Priority(9)]
    public class AddB : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Arguments[0] = (string)context.Arguments[0] + "B";
            context.Proceed();
        }
    }

    public class AddB2 : AddB
    { }

    [Priority(8)]
    public class AddC : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Arguments[0] = (string)context.Arguments[0] + "C";
            context.Proceed();
        }
    }

    public class AddC2 : AddC
    { }

    [Priority(7)]
    public class AddD : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Arguments[0] = (string)context.Arguments[0] + "D";
            context.Proceed();
        }
    }

    public class AddD2 : AddD
    { }

    [Priority(6)]
    public class AddE : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Arguments[0] = (string)context.Arguments[0] + "E";
            context.Proceed();
        }
    }

    public class AddE2 : AddE
    { }
}
