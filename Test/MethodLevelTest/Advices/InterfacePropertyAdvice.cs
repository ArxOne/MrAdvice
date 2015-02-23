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

    public class InterfacePropertyAdvice : Attribute, IPropertyAdvice
    {
        public int? NewReturnValue;

        public void Advise(PropertyAdviceContext context)
        {
            // some non-sense mocking
            if (context.IsGetter)
                context.ReturnValue = Activator.CreateInstance(context.TargetProperty.PropertyType);

            // now, some advice
            if (NewReturnValue.HasValue)
                context.ReturnValue = NewReturnValue.Value;
        }
    }
}