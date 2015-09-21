#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WeavingAdviceTest
    {
        public class MethodWeavingAdvice : Attribute, IMethodWeavingAdvice
        {
            public void Advise(MethodWeavingContext context)
            {
            }
        }

        public class WeavingAdvisedClass
        {
            [MethodWeavingAdvice]
            public void WeavingAdvisedMethod()
            {
            }
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void SimpleWeavingAdviceTest()
        {
            var c=new WeavingAdvisedClass();
        }
    }
}
