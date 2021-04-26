using System;
using ArxOne.MrAdvice.Advice;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MethodLevelTest
{
    [TestClass]
    public class AttributesTest
    {
        public class CountingAdvice : Attribute, IMethodAdvice
        {
            [ThreadStatic] public static int InstancesCount;

            public CountingAdvice()
            {
                InstancesCount++;
            }

            public void Advise(MethodAdviceContext context)
            {
                context.Proceed();
            }
        }

        [CountingAdvice]
        public void Advised()
        {
        }

        [TestMethod]
        [TestCategory("Attributes")]
        public void AttributeCreatedOnceTest()
        {
            CountingAdvice.InstancesCount = 0;
            Advised();
            Assert.AreEqual(1, CountingAdvice.InstancesCount);
        }
    }
}
