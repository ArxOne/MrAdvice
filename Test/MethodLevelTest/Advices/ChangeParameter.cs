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
    public class ChangeParameter : Attribute, IMethodAdvice
    {
        private int? _newParameter;
        private int? _newReturnValue;

        public int NewParameter
        {
            get { return _newParameter ?? -1; }
            set { _newParameter = value; }
        }

        public int NewReturnValue
        {
            get { return _newReturnValue ?? -1; }
            set { _newReturnValue = value; }
        }

        public void Advise(MethodAdviceContext call)
        {
            if (_newParameter.HasValue && call.Arguments.Count > 0)
                call.Arguments[0] = _newParameter.Value;
            call.Proceed();
            if (_newReturnValue.HasValue)
                call.ReturnValue = _newReturnValue.Value;
        }
    }
}
