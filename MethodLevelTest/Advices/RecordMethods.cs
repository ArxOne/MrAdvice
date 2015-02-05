
namespace MethodLevelTest.Advices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArxOne.Weavisor.Advice;

    public class RecordMethods : Attribute, IMethodInfoAdvice
    {
        public static readonly IList<MethodInfo> MethodInfos = new List<MethodInfo>();

        public void Advise(MethodBase methodBase)
        {
            var methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
                MethodInfos.Add(methodInfo);
        }
    }
}
