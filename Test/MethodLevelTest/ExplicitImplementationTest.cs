using ArxOne.MrAdvice.Advice;
using MethodLevelTest.Advices;

namespace MethodLevelTest
{
    using NUnit.Framework;
    using System;

    public class IncrementParameter : Attribute, IParameterAdvice
    {
        public void Advise(ParameterAdviceContext context)
        {
            context.Value = (int)context.Value + 1;
            context.Proceed();
        }
    }

    public interface IA
    {
        int F(int p);
        int X(int p);
    }

    [EmptyMethodAdvice]
    public class A : IA
    {
        int IA.F([IncrementParameter] int p) => F(p);
        public int X([IncrementParameter] int p) => p;

        private int F(int p) => p;
    }

    [TestFixture]
    [Category("ExplicitImplementation")]
    public class ExplicitImplementationTest
    {
        [Test]
        public void UseWeavedClassExplicitMarkerTest()
        {
            IA a = new A();
            var x = a.X(1);
            Assert.That(x, Is.EqualTo(2));
        }

        [Test]
        public void UseWeavedClassTest2()
        {
            IA a = new A();
            var x = a.F(1);
            Assert.That(x, Is.EqualTo(2));
        }
    }
}