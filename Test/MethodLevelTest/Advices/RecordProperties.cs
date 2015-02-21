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

    public class RecordProperties : Attribute, IPropertyInfoAdvice
    {
        public static readonly IList<PropertyInfo> PropertyInfos = new List<PropertyInfo>();

        public void Advise(PropertyInfoAdviceContext context)
        {
            PropertyInfos.Add(context.TargetProperty);
        }
    }
}
