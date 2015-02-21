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

    public class ParameterAdvice : Attribute, IParameterAdvice
    {
        public void Advise(ParameterAdviceContext context)
        {
            if (context.IsIn)
            {
                if (context.ParameterType == typeof(int))
                    context.SetValue(context.GetValue<int>() + 1);
                if (context.ParameterType == typeof(string))
                    context.SetValue(context.GetValue<string>() + "there");
            }
            context.Proceed();
            if (context.IsOut && !context.IsIn)
                context.SetValue(context.GetValue<int>() * 2);
        }
    }
}
