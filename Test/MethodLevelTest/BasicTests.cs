#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Advices;
    using ArxOne.MrAdvice;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class MethodAdvisedCtorClass
    {
        [RecordCall]
        public MethodAdvisedCtorClass()
        {
        }
    }

    [PInvokerAdvice]
    public class PInvoker
    {
        [DllImport("kernel32")]
        public static extern IntPtr GetCurrentProcess();
    }

    public class PInvokerAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
            context.ReturnValue = new IntPtr(1234);
        }
    }

    [TestClass]
    public class BasicTests
    {
        [EmptyMethodAdvice]
        public delegate void SomeDelegate();

        [TestMethod]
        [TestCategory("Weaving")]
        public void MethodTest()
        {
            new EmptyAdvisedClass().MethodTest();
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void IndirectMethodTest()
        {
            new EmptyAdvisedClass().IndirectMethodTest();
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void StaticMethodTest()
        {
            EmptyAdvisedClass.StaticMethodTest();
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void MethodWithParameterTest()
        {
            new EmptyAdvisedClass().MethodWithParameterTest(2);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void StaticMethodWithParameterTest()
        {
            EmptyAdvisedClass.StaticMethodWithParameterTest(3);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void PropertyAtMethodLevelTest()
        {
            var c = new EmptyAdvisedClass();
            c.Property++; // which calls a getter and a setter
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void PropertyAtPropertyLevelTest()
        {
            var c = new EmptyAdvisedClass();
            c.Property2++; // which calls a getter and a setter
        }

        [TestMethod]
        [TestCategory("Interception")]
        public void ResolveOverloadTest()
        {
            var r = EmptyAdvisedClass.Overload(2);
            Assert.AreEqual(2, r);
        }

        [TestMethod]
        [TestCategory("Constructor")]
        public void EmptyAdvisedWithMethodCtorTest()
        {
            var count = RecordCall.Count;
            var instance = new MethodAdvisedCtorClass();
            Assert.AreEqual(count + 1, RecordCall.Count);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void RefParameterTest()
        {
            var c = new EmptyAdvisedClass();
            int b = 10;
            var r = c.UsesRef(3, ref b);
            Assert.AreEqual(r, b);
            Assert.AreEqual(13, b);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void OutParameterTest()
        {
            var c = new EmptyAdvisedClass();
            int b;
            c.UsesOut(4, out b);
            Assert.AreEqual(4, b);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void OutGenericParameterTest()
        {
            var c = new EmptyAdvisedClass();
            List<int> b;
            c.UsesOut(new List<int> { 5 }, out b);
            Assert.AreEqual(1, b.Count);
            Assert.AreEqual(5, b[0]);
        }

        //[TestMethod]
        //public void NonGenericExpression()
        //{
        //    Z(new Action(NonGenericExpression));
        //    //Expression<Action> e = () => NonGenericExpression();
        //    //var m = (MethodInfo)MethodBase.GetCurrentMethod();
        //    //var f = Expression.Call(m,Expression.Constant(this));
        //}

        [TestMethod]
        [TestCategory("Weaving")]
        public void TryBlockUnusedTest()
        {
            new EmptyAdvisedClass().TryBlockUnused();
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void TryBlockUsedTest()
        {
            new EmptyAdvisedClass().TryBlockUsed();
        }

        [TestMethod]
        [TestCategory("Interception")]
        public void AsyncTest()
        {
            var c = new AdvisedClass();
            Assert.IsTrue(c.LaunchAsyncMethod());
        }


        private void Z(Delegate d)
        {
            Invocation.ProcessInfoAdvices(typeof(BasicTests));
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void MethodFromGenericClassTest()
        {
            new GenericEmptyAdvisedClass<int>().DoSomething();
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void MethodFromGenericClassWithParameterTest()
        {
            var r = new GenericEmptyAdvisedClass<int>().ReturnValue(12);
            Assert.AreEqual(12, r);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void TwoAspectsTest()
        {
            int c = RecordCall.Count;
            var r = new EmptyAdvisedClass();
            var z = r.ReturnParameter(10);
            Assert.AreEqual(2, z);
            Assert.AreEqual(c + 1, RecordCall.Count);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void PriorityTest()
        {
            var c = new AdvisedClass();
            var r = c.GetString("...");
            Assert.AreEqual("...ABCDE", r);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void InheritedPriorityTest()
        {
            var c = new AdvisedClass();
            var r = c.GetString2(":)");
            Assert.AreEqual(":)ABCDE", r);
        }

        [TestMethod]
        [TestCategory("Exception")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionTest()
        {
            try
            {
                var c = new AdvisedClass();
                c.ThrowInvalidOperationException();
            }
            catch
            {
                throw;
            }
        }

        [TestMethod]
        [TestCategory("Exception")]
        public void ExceptionWithStackTraceTest()
        {
            try
            {
                var c = new AdvisedClass();
                c.ThrowInvalidOperationException();
            }
            catch (InvalidOperationException ioe)
            {
                var topTrace = ioe.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[1];
                Assert.IsTrue(topTrace.Contains("ThrowInvalidOperationException"));
            }
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void MethodWithGenericParameterTest()
        {
            new EmptyAdvisedClass().MethodWithGenericParameterTest(6);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void StaticMethodWithGenericParameterTest()
        {
            var t = GenericEmptyAdvisedClass<int>.StaticMethod<string>("hop");
            Assert.AreEqual(typeof(int), t[0]);
            Assert.AreEqual(typeof(string), t[1]);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void ManyParametersTest()
        {
            var r = new EmptyAdvisedClass().Add(1, 2, 3, 4, 5);
            Assert.AreEqual(r, 1 + 2 + 3 + 4 + 5);
        }

        public class CtorClass
        {
            [EmptyMethodAdvice]
            public CtorClass(int a, int b, int c, int d, int e, int f, int g, int h, int i)
            { }
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void CtorWith8ArgumentsTest()
        {
            var r = new CtorClass(1, 2, 3, 4, 5, 6, 7, 8, 9);
        }

        public abstract class AbstractClass
        {
            [ChangeParameter(NewParameter = 10)]
            public abstract int Add(int a, int b);
        }

        private class ConcreteClass : AbstractClass
        {
            public override int Add(int a, int b)
            {
                return a + b;
            }
        }

        //[TestMethod]
        //[TestCategory("Weaving")]
        //public void AbstractTestTest()
        //{
        //    var c = new ConcreteClass();
        //    var r = c.Add(1, 2);
        //    Assert.AreEqual(12, r);
        //}

        [TestMethod]
        [TestCategory("Weaving")]
        public void OverloadedIndexerTest()
        {
            var o = new OverloadedIndexerAdvisedClass();
            Assert.AreEqual(10, o["dude"]);
            Assert.AreEqual(20, o[1234]);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void PInvokeTest()
        {
            var p = PInvoker.GetCurrentProcess();
            Assert.AreEqual(1234, p.ToInt32());
        }

        public class A
        {
            public int V;
        }

        [ChangeParameter(NewParameter = 12)]
        public string ConstrainedMethod<TValue>(int i, TValue v)
            where TValue : A
        {
            return i.ToString() + v.ToString();
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void GenericConstraintTest()
        {
            var r = ConstrainedMethod(3, new A());
            Assert.IsTrue(r.StartsWith("12"));
        }
        public interface IConstrainedInterface
        {
            TValue GetSomething<TValue>(int i, TValue v)
                where TValue : A;
        }

        [ChangeParameter(NewParameter = 34)]
        public class ConstrainedClass : IConstrainedInterface
        {
            public TValue GetSomething<TValue>(int i, TValue v)
                where TValue : A
            {
                v.V = i;
                return v;
            }
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void InterfaceGenericConstraintTest()
        {
            var c = new ConstrainedClass();
            var r = c.GetSomething(5, new A());
            Assert.AreEqual(34, r.V);
        }
    }
}
