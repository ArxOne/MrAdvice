#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System;
using ArxOne.MrAdvice.Advice;
using NUnit.Framework;

namespace MethodLevelTest
{
    [TestFixture]
    [Category("Attributes")]
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

        [Test]
        public void AttributeCreatedOnceTest()
        {
            CountingAdvice.InstancesCount = 0;
            Advised();
            Assert.That(CountingAdvice.InstancesCount, Is.EqualTo(1));
        }
    }
}