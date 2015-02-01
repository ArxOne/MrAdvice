#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Advices;
    using ArxOne.Weavisor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class EmptyAdvisedClass
    {
        [EmptyAdvice]
        public void MethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("MethodTest", thisMethod.Name);
        }

        [EmptyAdvice]
        public static void StaticMethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("StaticMethodTest", thisMethod.Name);
        }

        [EmptyAdvice]
        public void MethodWithParameterTest(int two)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("MethodWithParameterTest", thisMethod.Name);
            Assert.AreEqual(2, two);
        }

        [EmptyAdvice]
        public static void StaticMethodWithParameterTest(int three)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("StaticMethodWithParameterTest", thisMethod.Name);
            Assert.AreEqual(3, three);
        }

        [EmptyAdvice]
        public static int Overload(int a)
        {
            return a;
        }

        [EmptyAdvice]
        public static int Overload(int a, int b)
        {
            return a + b;
        }

        ////[EmptyAdvice]
        ////public void MethodWithGenericParameterTest<TValue>(TValue six)
        ////{
        ////    Assert.AreEqual(6, six);
        ////}
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
        [TestCategory("Interception")]
        public void ResolveOverloadTest()
        {
            var r = EmptyAdvisedClass.Overload(2);
            Assert.AreEqual(2, r);
        }

        ////[TestMethod]
        ////[TestCategory("Weaving")]
        ////public void MethodWithGenericParameterTest()
        ////{
        ////    new EmptyAdvisedClass().MethodWithGenericParameterTest(6);
        ////}
    }
}
