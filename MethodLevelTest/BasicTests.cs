#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using System.Reflection;
    using Advices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class EmptyAdvisedClass
    {
        [EmptyMethodAdvice]
        public void MethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("MethodTest", thisMethod.Name);
        }

        [EmptyMethodAdvice]
        public static void StaticMethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("StaticMethodTest", thisMethod.Name);
        }

        [EmptyMethodAdvice]
        public void MethodWithParameterTest(int two)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("MethodWithParameterTest", thisMethod.Name);
            Assert.AreEqual(2, two);
        }

        [EmptyMethodAdvice]
        public static void StaticMethodWithParameterTest(int three)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("StaticMethodWithParameterTest", thisMethod.Name);
            Assert.AreEqual(3, three);
        }

        [EmptyMethodAdvice]
        public static int Overload(int a)
        {
            return a;
        }

        [EmptyMethodAdvice]
        public static int Overload(int a, int b)
        {
            return a + b;
        }

        private int _property;

        [EmptyMethodAdvice]
        public int Property
        {
            get
            {
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.AreNotEqual("get_Property", thisMethod.Name);
                return _property;
            }
            set
            {
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.AreNotEqual("set_Property", thisMethod.Name);
                _property = value;
            }
        }

        ////[EmptyMethodAdvice]
        ////public void MethodWithGenericParameterTest<TValue>(TValue six)
        ////{
        ////    Assert.AreEqual(6, six);
        ////}
    }

    public class MethodAdvisedCtorClass
    {
        [RecordCall]
        public MethodAdvisedCtorClass()
        {
        }
    }

    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        [TestCategory("Weaving")]
        public void MethodTest()
        {
            new EmptyAdvisedClass().MethodTest();
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


        ////[TestMethod]
        ////[TestCategory("Weaving")]
        ////public void MethodWithGenericParameterTest()
        ////{
        ////    new EmptyAdvisedClass().MethodWithGenericParameterTest(6);
        ////}
    }
}
