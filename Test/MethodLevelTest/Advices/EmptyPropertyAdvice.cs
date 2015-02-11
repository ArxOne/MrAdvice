#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest.Advices
{
    using System;
    using ArxOne.MrAdvice.Advice;

    public class EmptyPropertyAdvice : Attribute, IPropertyAdvice
    {
        public void Advise(PropertyAdviceContext context)
        {
            context.Proceed();
        }
    }
}
