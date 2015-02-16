#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
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
            Assert.AreNotEqual("MethodTest", thisMethod.Name);
        }
    }
}
