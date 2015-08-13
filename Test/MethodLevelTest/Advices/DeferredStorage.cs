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

    [AbstractTarget]
    public class DeferredStorage : Attribute, IPropertyAdvice
    {
        private static object _value;

        public void Advise(PropertyAdviceContext context)
        {
            if (context.IsGetter)
                context.ReturnValue = _value;
            else
                _value = context.Value;
        }
    }
}
