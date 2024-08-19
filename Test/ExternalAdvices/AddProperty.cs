#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Reflection;

namespace ExternalAdvices
{
    using System;
    using ArxOne.MrAdvice.Advice;

    public class AddProperty : Attribute, IMethodWeavingAdvice
    {
        public void Advise(MethodWeavingContext context)
        {
            context.TypeWeaver.AddAutoProperty(context.TargetMethodName + "_Property", typeof(string), MethodAttributes.Public);
        }
    }
}
