
namespace MethodLevelTest.Advices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArxOne.Weavisor.Advice;

    public class RecordProperties : Attribute, IPropertyInfoAdvice
    {
        public static readonly IList<PropertyInfo> PropertyInfos = new List<PropertyInfo>();

        public void Advise(PropertyInfoAdviceContext context)
        {
            PropertyInfos.Add(context.TargetProperty);
        }
    }
}
