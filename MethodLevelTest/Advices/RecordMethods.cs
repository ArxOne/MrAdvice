
namespace MethodLevelTest.Advices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArxOne.Weavisor.Advice;

    public class RecordMethods : Attribute, IMethodInitializer
    {
        public static IList<MethodInfo> MethodInfos = new List<MethodInfo>();

        public void Initialize(MethodInfo methodInfo1)
        {
            var methodInfo = methodInfo1 as MethodInfo;
            if (methodInfo != null)
                MethodInfos.Add(methodInfo);
        }
    }
}
