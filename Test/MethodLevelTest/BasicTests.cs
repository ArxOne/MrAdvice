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
    using NUnit.Framework;

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

    [TestFixture]
    public class BasicTests
    {
        [EmptyMethodAdvice]
        public delegate void SomeDelegate();

        [Test]
        [Category("Weaving")]
        public void MethodTest()
        {
            new EmptyAdvisedClass().MethodTest();
        }

        [Test]
        [Category("Weaving")]
        public void IndirectMethodTest()
        {
            new EmptyAdvisedClass().IndirectMethodTest();
        }

        [Test]
        [Category("Weaving")]
        public void StaticMethodTest()
        {
            EmptyAdvisedClass.StaticMethodTest();
        }

        [Test]
        [Category("Weaving")]
        public void MethodWithParameterTest()
        {
            new EmptyAdvisedClass().MethodWithParameterTest(2);
        }

        [Test]
        [Category("Weaving")]
        public void StaticMethodWithParameterTest()
        {
            EmptyAdvisedClass.StaticMethodWithParameterTest(3);
        }

        [Test]
        [Category("Weaving")]
        public void PropertyAtMethodLevelTest()
        {
            var c = new EmptyAdvisedClass();
            c.Property++;
        }

        [Test]
        [Category("Weaving")]
        public void PropertyAtPropertyLevelTest()
        {
            var c = new EmptyAdvisedClass();
            c.Property2++;
        }

        [Test]
        [Category("Interception")]
        public void ResolveOverloadTest()
        {
            var r = EmptyAdvisedClass.Overload(2);
            Assert.That(r, Is.EqualTo(2));
        }

        [Test]
        [Category("Constructor")]
        public void EmptyAdvisedWithMethodCtorTest()
        {
            var count = RecordCall.Count;
            var instance = new MethodAdvisedCtorClass();
            Assert.That(RecordCall.Count, Is.EqualTo(count + 1));
        }

        [Test]
        [Category("Weaving")]
        public void RefParameterTest()
        {
            var c = new EmptyAdvisedClass();
            int b = 10;
            var r = c.UsesRef(3, ref b);
            Assert.That(b, Is.EqualTo(r));
            Assert.That(b, Is.EqualTo(13));
        }

        [Test]
        [Category("Weaving")]
        public void OutParameterTest()
        {
            var c = new EmptyAdvisedClass();
            int b;
            c.UsesOut(4, out b);
            Assert.That(b, Is.EqualTo(4));
        }

        [Test]
        [Category("Weaving")]
        public void OutGenericParameterTest()
        {
            var c = new EmptyAdvisedClass();
            List<int> b;
            c.UsesOut(new List<int> { 5 }, out b);
            Assert.That(b.Count, Is.EqualTo(1));
            Assert.That(b[0], Is.EqualTo(5));
        }

        [Test]
        [Category("Weaving")]
        public void TryBlockUnusedTest()
        {
            new EmptyAdvisedClass().TryBlockUnused();
        }

        [Test]
        [Category("Weaving")]
        public void TryBlockUsedTest()
        {
            new EmptyAdvisedClass().TryBlockUsed();
        }

        [Test]
        [Category("Interception")]
        public void AsyncTest()
        {
            var c = new AdvisedClass();
            Assert.That(c.LaunchAsyncMethod(), Is.True);
        }


        [Test]
        [Category("Weaving")]
        public void MethodFromGenericClassTest()
        {
            new GenericEmptyAdvisedClass<int>().DoSomething();
        }

        [Test]
        [Category("Weaving")]
        public void MethodFromGenericClassWithParameterTest()
        {
            var r = new GenericEmptyAdvisedClass<int>().ReturnValue(12);
            Assert.That(r, Is.EqualTo(12));
        }

        [Test]
        [Category("Weaving")]
        public void TwoAspectsTest()
        {
            int c = RecordCall.Count;
            var r = new EmptyAdvisedClass();
            var z = r.ReturnParameter(10);
            Assert.That(z, Is.EqualTo(2));
            Assert.That(RecordCall.Count, Is.EqualTo(c + 1));
        }

        [Test]
        [Category("Weaving")]
        public void PriorityTest()
        {
            var c = new AdvisedClass();
            var r = c.GetString("...");
            Assert.That(r, Is.EqualTo("...ABCDE"));
        }

        [Test]
        [Category("Weaving")]
        public void InheritedPriorityTest()
        {
            var c = new AdvisedClass();
            var r = c.GetString2(":)");
            Assert.That(r, Is.EqualTo(":)ABCDE"));
        }

        [Test]
        [Category("Exception")]
        public void ExceptionTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var c = new AdvisedClass();
                c.ThrowInvalidOperationException();
            });
        }

        [Test]
        [Category("Exception")]
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
                if (!topTrace.Contains("ThrowInvalidOperationException"))
                    Assert.Inconclusive($"stack trace contains {ioe.StackTrace}");
            }
        }

        [Test]
        [Category("Weaving")]
        public void MethodWithGenericParameterTest()
        {
            new EmptyAdvisedClass().MethodWithGenericParameterTest(6);
        }

        [Test]
        [Category("Weaving")]
        public void StaticMethodWithGenericParameterTest()
        {
            var t = GenericEmptyAdvisedClass<int>.StaticMethod<string>("hop");
            Assert.That(t[0], Is.EqualTo(typeof(int)));
            Assert.That(t[1], Is.EqualTo(typeof(string)));
        }

        [Test]
        [Category("Weaving")]
        public void ManyParametersTest()
        {
            var r = new EmptyAdvisedClass().Add(1, 2, 3, 4, 5);
            Assert.That(r, Is.EqualTo(1 + 2 + 3 + 4 + 5));
        }

        public class CtorClass
        {
            [EmptyMethodAdvice]
            public CtorClass(int a, int b, int c, int d, int e, int f, int g, int h, int i)
            {
            }
        }

        [Test]
        [Category("Weaving")]
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

        [Test]
        [Category("Weaving")]
        public void OverloadedIndexerTest()
        {
            var o = new OverloadedIndexerAdvisedClass();
            Assert.That(o["dude"], Is.EqualTo(10));
            Assert.That(o[1234], Is.EqualTo(20));
        }

        [Test]
        [Category("Weaving")]
        public void PInvokeTest()
        {
            var p = PInvoker.GetCurrentProcess();
            Assert.That(p.ToInt32(), Is.EqualTo(1234));
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

        [Test]
        [Category("Weaving")]
        public void GenericConstraintTest()
        {
            var r = ConstrainedMethod(3, new A());
            Assert.That(r, Does.StartWith("12"));
        }

        [ChangeParameter(NewParameter = 12)]
        public string RefConstrainedMethod<TValue>(int i, ref TValue v)
            where TValue : struct
        {
            var o = (object)v;
            return i.ToString() + v.ToString();
        }

        [Test]
        [Category("Weaving")]
        public void RefGenericConstraintTest()
        {
            double d = -4;
            var r = RefConstrainedMethod(3, ref d);
            Assert.That(r, Does.StartWith("12-4"));
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

        [Test]
        [Category("Weaving")]
        public void InterfaceGenericConstraintTest()
        {
            var c = new ConstrainedClass();
            var r = c.GetSomething(5, new A());
            Assert.That(r.V, Is.EqualTo(34));
        }

        [EmptyMethodAdvice]
        public void F(byte? a)
        {
        }

        [Test]
        [Category("Weaving")]
        public void InNullableByteTest()
        {
            F(1);
        }

        [EmptyMethodAdvice]
        public void FR(ref byte? a)
        {
            if (a.HasValue)
                a++;
        }

        [Test]
        [Category("Weaving")]
        public void RefNullableByteTest()
        {
            byte? a = 12;
            FR(ref a);
            Assert.That(a, Is.EqualTo((byte?)13));
        }

        [EmptyMethodAdvice]
        public void FO(out byte? a)
        {
            a = 34;
        }

        [Test]
        [Category("Weaving")]
        public void OutNullableByteTest()
        {
            byte? a;
            FO(out a);
            Assert.That(a, Is.EqualTo((byte?)34));
        }

        [EmptyMethodAdvice]
        public class Ctor
        {
            public Ctor(object o)
            {
                Assert.That(o, Is.Not.Null);
            }
        }

        [Test]
        [Category("Weaving")]
        public void NonNullCtorParameterTest()
        {
            var c = new Ctor(new object());
        }

        public struct TestStruct
        {
            [EmptyMethodAdvice]
            public int N { get; set; }

            public int I;

            public void F()
            {
                var o = (object)this;
                var z = (TestStruct)o;
            }
        }

        [Test]
        [Category("Weaving")]
        public void StructSetterTest()
        {
            var r = new TestStruct();
            r.N = 1;
        }

        [EmptyMethodAdvice]
        public void UseStruct(TestStruct a)
        {
            Assert.That(a.I, Is.EqualTo(1));
            a.I = 2;
        }

        [Test]
        [Category("Weaving")]
        public void UseStructTest()
        {
            var s = new TestStruct { I = 1 };
            UseStruct(s);
            Assert.That(s.I, Is.EqualTo(1));
        }

        [AttributeUsage(AttributeTargets.Method), ArxOne.MrAdvice.Annotation.Priority(Priority)]
        public class Advice1Attribute : Attribute, IMethodInfoAdvice
        {
            public void Advise(MethodInfoAdviceContext context)
            {
                if (MethodInfoAdviceTests.Expected != Priority)
                    throw new InvalidOperationException();

                MethodInfoAdviceTests.Expected = Priority - 1;
            }

            public const int Priority = 1;
        }

        [AttributeUsage(AttributeTargets.Method), ArxOne.MrAdvice.Annotation.Priority(Priority)]
        public class Advice2Attribute : Attribute, IMethodInfoAdvice
        {
            public void Advise(MethodInfoAdviceContext context)
            {
                if (MethodInfoAdviceTests.Expected != Priority)
                    throw new InvalidOperationException();

                MethodInfoAdviceTests.Expected = Priority - 1;
            }

            public const int Priority = 2;
        }

        class ClassA
        {
            [Advice1, Advice2]
            public static void Method() { }
        }

        class ClassB
        {
            [Advice2, Advice1]
            public static void Method() { }
        }

        [TestFixture]
        [Category("Priority")]
        public class MethodInfoAdviceTests
        {
            [Test]
            public void MethodInfoAdviceClassA_MethodA()
            {
                Expected = 2;
                ClassA.Method();
                Assert.That(Expected, Is.EqualTo(0));
            }

            [Test]
            public void MethodInfoAdviceClassB_MethodB()
            {
                Expected = 2;
                ClassB.Method();
                Assert.That(Expected, Is.EqualTo(0));
            }

            public static int Expected;
        }
    }
}