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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
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

        [TestMethod]
        [TestCategory("Pointcut selection")]
        public void SetterRulesTest()
        {
            var t = typeof(SetterAdvisedType);
            Assert.IsNull(ArxOne.MrAdvice.Advices.Get(t.GetConstructor(new Type[0])));
            Assert.IsNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(SetterAdvisedType.ZetA))));
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(SetterAdvisedType.SetA))));
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(SetterAdvisedType.SetB))));
        }

        [TestMethod]
        [TestCategory("Pointcut selection")]
        public void ExcludeAdviceTest()
        {
            var t = typeof(AnyAdvisedType);
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(t.GetConstructor(new Type[0])));
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(AnyAdvisedType.F))));
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(AnyAdvisedType.G))));
            Assert.IsNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(AnyAdvisedType.H))));
        }

        [TestMethod]
        [TestCategory("Pointcut selection")]
        public void PublicAdviceTest()
        {
            var t = typeof(PublicAdvisedType);
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(t.GetConstructor(new Type[0])));
            Assert.IsNotNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod(nameof(PublicAdvisedType.Public))));
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            Assert.IsNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod("Protected", bindingFlags)));
            Assert.IsNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod("Private", bindingFlags)));
            Assert.IsNull(ArxOne.MrAdvice.Advices.Get(t.GetMethod("Internal", bindingFlags)));
        }
    }
}
