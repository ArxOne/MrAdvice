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
    using NUnit.Framework;

    public class GenericEmptyAdvisedClass<TValue>
    {
        [EmptyMethodAdvice]
        public void DoSomething()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.That(thisMethod.Name, Is.Not.EqualTo("DoSomething"));
        }

        public TValue ReturnValueNoAdvice(TValue value)
        {
            return (TValue)GetObjectValue(value);
        }

        private static object GetObjectValue(TValue value)
        {
            return value;
        }

        [EmptyMethodAdvice]
        public TValue ReturnValue(TValue value)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.That(thisMethod.Name, Is.Not.EqualTo("ReturnValue"));
            return value;
        }

        [EmptyMethodAdvice]
        public static Type[] StaticMethod<T1>(T1 t)
        {
            return new[] { typeof(TValue), typeof(T1) };
        }
    }
}