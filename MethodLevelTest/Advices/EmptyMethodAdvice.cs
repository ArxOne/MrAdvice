#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest.Advices
{
    using System;
    using ArxOne.Weavisor.Advice;

    public class EmptyMethodAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext call)
        {
            call.Proceed();
        }
    }
}