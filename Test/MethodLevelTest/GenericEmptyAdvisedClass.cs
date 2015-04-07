#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using System;
    using System.Reflection;
    using Advices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class GenericEmptyAdvisedClass<TValue>
    {
        [EmptyMethodAdvice]
        public void DoSomething()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("DoSomething", thisMethod.Name);
        }

        public TValue ReturnValueNoAdvice(TValue value)
        {
            return (TValue) GetObjectValue(value);
        }

        private static object GetObjectValue(TValue value)
        {
            return value;
        }

        [EmptyMethodAdvice]
        public TValue ReturnValue(TValue value)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("ReturnValue", thisMethod.Name);
            return value;
        }

        [EmptyMethodAdvice]
        public static Type[] StaticMethod<T1>(T1 t)
        {
            return new[] { typeof(TValue), typeof(T1) };
        }
    }
}
