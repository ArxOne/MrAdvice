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
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AdvicesTest
    {
        public class EmptyAdvice : Attribute, IMethodAdvice
        {
            public void Advise(MethodAdviceContext context)
            {
                context.Proceed();
            }
        }

        [EmptyAdvice]
        public void Advised()
        {
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(MethodBase.GetCurrentMethod()));
        }

        public void NotAdvised()
        {
            Assert.IsNull(ArxOne.MrAdvice.Advices.Get(MethodBase.GetCurrentMethod()));
        }

        [TestMethod]
        public void AdvisedFromOutsideTest()
        {
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(GetType().GetMethod(nameof(Advised))));
        }

        [TestMethod]
        public void AdvisedFromInsideTest()
        {
            Advised();
        }

        [TestMethod]
        public void NotAdvisedFromOutsideTest()
        {
            Assert.IsNull(ArxOne.MrAdvice.Advices.Get(GetType().GetMethod(nameof(NotAdvised))));
        }

        [TestMethod]
        public void NotAdvisedFromInsideTest()
        {
            NotAdvised();
        }
    }
}
