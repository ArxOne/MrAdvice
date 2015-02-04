
namespace MethodLevelTest.Advices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArxOne.Weavisor.Initializer;

    public class RecordProperties : Attribute, IPropertyInitializer
    {
        public static readonly IList<PropertyInfo> PropertyInfos = new List<PropertyInfo>();

        public void Initialize(PropertyInfo propertyInfo)
        {
            PropertyInfos.Add(propertyInfo);
        }
    }
}
