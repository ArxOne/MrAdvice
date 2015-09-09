#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace UwpTest
{
    using System;
    using System.Reflection;
    using ArxOne.MrAdvice.Advice;

    public class UwpAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
            if (((MethodInfo) context.TargetMethod).ReturnType == typeof (int))
            {
                context.ReturnValue = (int) context.ReturnValue + 1;
            }
        }
    }
}