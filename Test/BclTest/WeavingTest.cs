#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace BclTest
{
    using System;
    using System.Threading.Tasks;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;


    public class SomeAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
            context.ReturnValue = (int)context.ReturnValue + 2;
        }
    }

    [TestClass]
    public class WeavingTest
    {
        [SomeAdvice]
        public int F(int a) => a + 1;

        [TestMethod]
        [TestCategory("Weaving")]
        public void BclAdviceTest()
        {
            var t = typeof (TaskEx);
            var r = F(10);
            Assert.AreEqual(13, r);
        }
    }
}
