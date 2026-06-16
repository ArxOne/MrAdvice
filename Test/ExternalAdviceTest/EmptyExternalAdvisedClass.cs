#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using NUnit.Framework;

namespace ExternalAdviceTest
{
    using System.Reflection;
    using ExternalAdvices;

    public class EmptyExternalAdvisedClass
    {
        [ExternalEmptyAdvice]
        public void MethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            F(thisMethod);
            Assert.That(thisMethod.Name, Is.Not.EqualTo("MethodTest"));
        }

        [AddProperty]
        public void WeavingAdvisedMethodTest()
        {
            var property = GetType().GetProperty("WeavingAdvisedMethodTest_Property");
            F(property);
            Assert.That(property, Is.Not.Null);
        }

        void F(object o)
        { }
    }
}
