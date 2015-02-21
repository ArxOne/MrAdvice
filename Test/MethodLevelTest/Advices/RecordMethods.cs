#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest.Advices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArxOne.MrAdvice.Advice;

    public class RecordMethods : Attribute, IMethodInfoAdvice
    {
        public static readonly IList<MethodInfo> MethodInfos = new List<MethodInfo>();

        public void Advise(MethodInfoAdviceContext context)
        {
            var methodInfo = context.TargetMethod as MethodInfo;
            if (methodInfo != null)
                MethodInfos.Add(methodInfo);
        }
    }
}
