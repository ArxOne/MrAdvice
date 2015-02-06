#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest.Advices
{
    using System;
    using ArxOne.Weavisor.Advice;

    public class ChangeParameter : Attribute, IMethodAdvice
    {
        private int? _newParameter;
        private int? _newReturnValue;

        public int NewParameter
        {
            get { return _newParameter??-1; }
            set { _newParameter = value; }
        }

        public int NewReturnValue
        {
            get { return _newReturnValue??-1; }
            set { _newReturnValue = value; }
        }

        public void Advise(MethodAdviceContext call)
        {
            if (_newParameter.HasValue)
                call.Parameters[0] = _newParameter.Value;
            call.Proceed();
            if (_newReturnValue.HasValue)
                call.ReturnValue = _newReturnValue.Value;
        }
    }
}
