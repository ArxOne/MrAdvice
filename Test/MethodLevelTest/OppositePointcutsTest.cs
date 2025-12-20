namespace MethodLevelTest
{
    using System;
    using System.Reflection;
    using ArxOne.MrAdvice.Advice;
    using ArxOne.MrAdvice.Annotation;
    using NUnit.Framework;

    [AdviceA, AdviceB]
    public static class OppositePointcuts
    {
        public static void AdvisedOnce()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Assert.That(currentMethod.Name, Is.Not.EqualTo(nameof(AdvisedOnce)));
        }
    }

    [TestFixture]
    [Category("Pointcut selection")]
    public class OppositePointcutsTest
    {
        [Test]
        public void AdvisedOnceTest()
        {
            var m = typeof(OppositePointcuts).GetMethod(nameof(OppositePointcuts.AdvisedOnce));
            m.Invoke(null, Array.Empty<object>());
            var advices = ArxOne.MrAdvice.Advices.Get(m);

            Assert.That(advices.Length, Is.EqualTo(1));
            Assert.That(advices[0].GetType(), Is.EqualTo(typeof(AdviceA)));
        }
    }

    [ExcludePointcut("*AdvisedOnceTest")]
    public class AdviceA : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            Console.WriteLine("Entry From Advice A");
            context.Proceed();
            Console.WriteLine("Exit From Advice A");
        }
    }

    [IncludePointcut("*AdvisedOnceTest")]
    public class AdviceB : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            Console.WriteLine("Entry From Advice B");
            context.Proceed();
            Console.WriteLine("Exit From Advice B");
        }
    }
}