namespace MethodLevelTest
{
    using System;
    using System.Reflection;
    using ArxOne.MrAdvice.Advice;
    using ArxOne.MrAdvice.Annotation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [AdviceA, AdviceB]
    public static class OppositePointcuts
    {
        public static void AdvisedOnce()
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual(nameof(AdvisedOnce), currentMethod.Name);
        }
    }

    [TestClass]
    public class OppositePointcutsTest
    {
        [TestMethod]
        [TestCategory("Pointcut selection")]
        public void AdvisedOnceTest()
        {
            var m = typeof(OppositePointcuts).GetMethod(nameof(OppositePointcuts.AdvisedOnce));
            m.Invoke(null, new object[0]);
            var advices = ArxOne.MrAdvice.Advices.Get(m);
            Assert.AreEqual(1, advices.Length);
            Assert.AreEqual(typeof(AdviceA), advices[0].GetType());
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