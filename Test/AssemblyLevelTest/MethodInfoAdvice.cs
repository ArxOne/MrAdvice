#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace AssemblyLevelTest
{
    using System;
    using ArxOne.MrAdvice.Advice;

    public class MethodInfoAdvice : Attribute, IMethodInfoAdvice
    {
        public void Advise(MethodInfoAdviceContext context)
        {
        }
    }
}