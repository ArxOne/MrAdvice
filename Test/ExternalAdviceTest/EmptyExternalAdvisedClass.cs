#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ExternalAdviceTest
{
    using System.Reflection;
    using ExternalAdvices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class EmptyExternalAdvisedClass
    {
        [ExternalEmptyAdvice]
        public void MethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            F(thisMethod);
            Assert.AreNotEqual("MethodTest", thisMethod.Name);
        }

        [AddProperty]
        public void WeavingAdvisedMethodTest()
        {
            var property = GetType().GetProperty("WeavingAdvisedMethodTest_Property");
            F(property);
            Assert.IsNotNull(property);
        }

        void F(object o)
        { }
    }
}
