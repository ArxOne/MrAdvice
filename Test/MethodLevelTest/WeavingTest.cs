#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using ArxOne.MrAdvice;
    using ArxOne.MrAdvice.Advice;
    using ArxOne.MrAdvice.Annotation;
    using ExternalAdvices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // https://github.com/ArxOne/MrAdvice/issues/32
    public class Test
    {
        [MyProudAdvice] // this will pass
        public void Method1(FooClass fooClass)
        {
        }

        [MyProudAdvice] // this will pass
        public void Method1A(FooClass foo1, FooClass foo2)
        {
        }

        [MyProudAdvice] // fody error
        public void Method2(FooClass fooClass, string name, long id, long count)
        {
        }

        [MyProudAdvice] // fody error
        public void Method3(string name, FooClass fooClass, long id, long count)
        {
        }

        [MyProudAdvice] // fody error
        public void Method4(string name, long id, FooClass fooClass, long count)
        {
        }

        [MyProudAdvice] // fody error
        public void Method5(string name, long id, long count, FooClass fooClass)
        {
        }

        [MyProudAdvice] // fody error
        public void Method6(FooStruct fooStruct, string name, long id, long count)
        {
        }

        [MyProudAdvice] // fody error
        public void Method6(FooEnum fooEnum, string name, long id, long count)
        {
        }
    }

    public enum FooEnum
    {

    }

    public struct FooStruct
    {
        public int Id { get; set; }
    }

    public class FooClass
    {
        public int Id { get; set; }
    }

    public class MyProudAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            // do things you want here
            context.Proceed(); // this calls the original method
            // do other things here
        }
    }

    public interface ISomething<TValue>
    {
    }

    [MyProudAdvice]
    public class Something : ISomething<int>
    {
    }

    public class ConcreteMethodAdvice : AbstractMethodAdvice
    {
        public override void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }

    public class ConcreteWeavedClass
    {
        [ConcreteMethodAdvice]
        public void AdvisedMethod()
        {
        }

        [ExternalConcreteMethodAdvice]
        public void ExternalAdvisedMethod()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class InheritableAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NonInheritableAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }

    [InheritableAdvice]
    public class InheritableTestClass
    {
        public virtual void VF()
        {
        }
    }

    public class InheritableDerivedTestClass : InheritableTestClass
    {
        public override void VF()
        {
        }
    }

    [NonInheritableAdvice]
    public class NonInheritableTestClass
    {
        public virtual void VF()
        {
        }
    }

    public class NonInheritableDerivedTestClass : NonInheritableTestClass
    {
        public override void VF()
        {
        }
    }

    [TestClass]
    public class InheritanceTest
    {
        [TestMethod]
        public void InheritedTest()
        {
            var m = typeof(InheritableDerivedTestClass).GetMethod("VF");
            var advices = ArxOne.MrAdvice.Advices.Get(m);
            Assert.IsNotNull(advices);
            Assert.AreEqual(1, advices.Length);
        }

        [TestMethod]
        public void NotInheritedBaseTest()
        {
            var m = typeof(NonInheritableTestClass).GetMethod("VF");
            var advices = ArxOne.MrAdvice.Advices.Get(m);
            Assert.IsNotNull(advices);
            Assert.AreEqual(1, advices.Length);
        }

        [TestMethod]
        public void NotInheritedTest()
        {
            var m = typeof(NonInheritableDerivedTestClass).GetMethod("VF");
            var advices = ArxOne.MrAdvice.Advices.Get(m);
            Assert.IsNull(advices);
        }
    }

    public struct RawStruct
    {
        public int Z { get; set; }

        public RawStruct(int z)
        {
            Z = z;
        }
    }

    [ExternalEmptyAdvice]
    public struct EmptyAdvisedStruct
    {
        public int Z { get; set; }

        public EmptyAdvisedStruct(int z)
        {
            Z = z;
        }
    }

    public class CountAccesses : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            dynamic o = context.Target;
            o.Access++;
            context.Proceed();
        }
    }


    public struct CountAdvisedStruct
    {
        public int Access;

        public int Z { get; [CountAccesses] set; }

        public CountAdvisedStruct(int z)
        {
            Access = 0;
            Z = z;
        }

        public void F()
        {
            object o = this;
        }
    }

    public struct CountAdvisedStruct<T, T2>
    {
        public int Access;

        public int Z { get; [CountAccesses] set; }

        public CountAdvisedStruct(int z)
        {
            Access = 0;
            Z = z;
        }

        public void F()
        {
            object o = this;
        }
    }

    [TestClass]
    public class StructTest
    {
        [TestMethod]
        public void RawMethodTest()
        {
            RawStruct s = new RawStruct(10);
            s.Z++;
            Assert.AreEqual(11, s.Z);
        }

        [TestMethod]
        public void AdvisedMethodTest()
        {
            var s = new EmptyAdvisedStruct(20);
            s.Z++;
            Assert.AreEqual(21, s.Z);
        }

        [TestMethod]
        public void CountAdvisedMethodTest()
        {
            var s = new CountAdvisedStruct(30);
            s.Z++;
            Assert.AreEqual(31, s.Z);
            Assert.AreEqual(2, s.Access);
        }

        [TestMethod]
        public void CountAdvisedMethodOnGenericStructTest()
        {
            var s = new CountAdvisedStruct<int, float>(30);
            s.Z++;
            Assert.AreEqual(31, s.Z);
            Assert.AreEqual(2, s.Access);
        }
    }

    public class ExternalPropertyAdvice : Attribute, IMethodAdvice
    {
        public ConsoleColor Color { get; set; }

        public void Advise(MethodAdviceContext context)
        {
        }
    }

    public class WeavedWithExternal
    {
        [ExternalPropertyAdvice(Color = ConsoleColor.Black)]
        public int Z { get; set; }
    }

    [TestClass]
    public class MiscTest
    {
        [TestMethod]
        public void WeavedWithExternalTest()
        {
            var w = new WeavedWithExternal();
        }
    }

    [IncludePointcut(Scope = VisibilityScope.PublicMember)]
    public class PublicOnlyAdvice : Attribute, IMethodAdvice
    {
        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        /// <code></code>
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
            context.ReturnValue = (int)context.ReturnValue + 1;
        }
    }

    [PublicOnlyAdvice]
    public class UnadvisedClass
    {
        internal UnadvisedClass() { }

        internal int F(int a)
        {
            return a + 1;
        }
    }

    [TestClass]
    public class PointcutTests
    {
        [TestMethod]
        public void UnadvisedTest()
        {
            var u = new UnadvisedClass();
            var r = u.F(1);
            Assert.AreEqual(2, r);
        }
    }
}
