#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest.Advices
{
    using System;
    using System.Reflection;
    using ArxOne.MrAdvice.Advice;

    public class InterfaceMethodAdvice : Attribute, IMethodAdvice
    {
        public int? NewReturnValue;
        public int? NewFirstParameter;

        public void Advise(MethodAdviceContext context)
        {
            // some non-sense mocking
            var methodInfo = (MethodInfo)context.TargetMethod;
            var returnType = methodInfo.ReturnType;
            if (returnType != typeof(void))
                context.ReturnValue = Activator.CreateInstance(returnType);

            // now, some advice
            if (NewFirstParameter.HasValue)
                context.Arguments[0] = NewFirstParameter.Value;
            if (NewReturnValue.HasValue)
                context.ReturnValue = NewReturnValue.Value;
        }
    }
}
