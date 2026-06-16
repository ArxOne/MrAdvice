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
    using NUnit.Framework;

    [TestFixture]
    [Category("Advices")]
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
            Assert.That(ArxOne.MrAdvice.Advices.Get(MethodBase.GetCurrentMethod()), Is.Not.Null);
        }

        public void NotAdvised()
        {
            Assert.That(ArxOne.MrAdvice.Advices.Get(MethodBase.GetCurrentMethod()), Is.Null);
        }

        [Test]
        public void AdvisedFromOutsideTest()
        {
            Assert.That(ArxOne.MrAdvice.Advices.Get(GetType().GetMethod(nameof(Advised))), Is.Not.Null);
        }

        [Test]
        public void AdvisedFromInsideTest()
        {
            Advised();
        }

        [Test]
        public void NotAdvisedFromOutsideTest()
        {
            Assert.That(ArxOne.MrAdvice.Advices.Get(GetType().GetMethod(nameof(NotAdvised))), Is.Null);
        }

        [Test]
        public void NotAdvisedFromInsideTest()
        {
            NotAdvised();
        }
    }
}