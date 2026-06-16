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
    using ArxOne.MrAdvice.Annotation;
    using NUnit.Framework;

    [TestFixture]
    [Category("Pointcut selection")]
    public class PointcutRulesTest
    {
        [IncludePointcut("*.Set@")]
        public class SetterAdvice : Attribute, IMethodAdvice
        {
            public void Advise(MethodAdviceContext context)
            {
                context.Proceed();
            }
        }

        public class AnyAdvice : Attribute, IMethodAdvice
        {
            public void Advise(MethodAdviceContext context)
            {
                context.Proceed();
            }
        }

        [IncludePointcut(Scope = VisibilityScope.PublicMember)]
        public class PublicAdvice : Attribute, IMethodAdvice
        {
            public void Advise(MethodAdviceContext context)
            {
                context.Proceed();
            }
        }

        [SetterAdvice]
        public class SetterAdvisedType
        {
            public void SetA() { }
            public void ZetA() { }
            public void SetB() { }
        }

        [AnyAdvice]
        public class AnyAdvisedType
        {
            public void F() { }
            public void G() { }

            [ExcludeAdvices("*+AnyAdvice")]
            public void H() { }
        }

        [PublicAdvice]
        public class PublicAdvisedType
        {
            public void Public() { }
            protected void Protected() { }
            private void Private() { }
            private void Internal() { }
        }

        [Test]
        public void SetterRulesTest()
        {
            var t = typeof(SetterAdvisedType);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetConstructor(new Type[0])), Is.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(SetterAdvisedType.ZetA))), Is.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(SetterAdvisedType.SetA))), Is.Not.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(SetterAdvisedType.SetB))), Is.Not.Null);
        }

        [Test]
        public void ExcludeAdviceTest()
        {
            var t = typeof(AnyAdvisedType);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetConstructor(new Type[0])), Is.Not.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(AnyAdvisedType.F))), Is.Not.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(AnyAdvisedType.G))), Is.Not.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(AnyAdvisedType.H))), Is.Null);
        }

        [Test]
        public void PublicAdviceTest()
        {
            var t = typeof(PublicAdvisedType);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetConstructor(new Type[0])), Is.Not.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(PublicAdvisedType.Public))), Is.Not.Null);

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod("Protected", bindingFlags)), Is.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod("Private", bindingFlags)), Is.Null);
            Assert.That(ArxOne.MrAdvice.Advices.Get(t.GetMethod("Internal", bindingFlags)), Is.Null);
        }
    }
}