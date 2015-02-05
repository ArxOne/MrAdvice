
namespace MethodLevelTest.Advices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArxOne.Weavisor.Initializer;

    public class RecordMethods : Attribute, IMethodInitializer
    {
        public static readonly IList<MethodInfo> MethodInfos = new List<MethodInfo>();

        public void Initialize(MethodBase methodBase)
        {
            var methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
                MethodInfos.Add(methodInfo);
        }
    }
}
