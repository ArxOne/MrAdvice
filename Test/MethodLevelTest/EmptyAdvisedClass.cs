#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using System.Collections.Generic;
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

        [EmptyMethodAdvice2]
        public void IndirectMethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("IndirectMethodTest", thisMethod.Name);
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

        [EmptyPropertyAdvice]
        public int Property2
        {
            get
            {
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.AreNotEqual("get_Property2", thisMethod.Name);
                return _property;
            }
            set
            {
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.AreNotEqual("set_Property2", thisMethod.Name);
                _property = value;
            }
        }

        [EmptyMethodAdvice]
        public int UsesRef(int a, ref int b)
        {
            b = a + b;
            return b;
        }

        [EmptyMethodAdvice]
        public void UsesOut(int a, out int b)
        {
            b = a;
        }

        [EmptyMethodAdvice]
        public void UsesOut(List<int> a, out List<int> b)
        {
            b = a;
        }

        [EmptyMethodAdvice]
        public void TryBlockUnused()
        {
            Overload(1);
        }

        [EmptyMethodAdvice]
        public void TryBlockUsed()
        {
            try
            {
                Overload(1);
            }
            finally
            {
            }
        }

        public void TryBlocksTest()
        {
            TryBlockUsed();
            TryBlockUnused();
        }

        [return: EmptyParameterAdvice]
        public void ReturnAdvisedMethod()
        { }

        public void ParameterAdvisedMethod([EmptyParameterAdvice] int a)
        { }

        public string ReturnAdvisedProperty { [return: EmptyParameterAdvice]get; set; }

        public string this[[EmptyParameterAdvice]string index]
        { get { return null; } set { } }

        [EmptyMethodAdvice]
        public void MethodWithGenericParameterTest<TValue>(TValue six)
        {
            Assert.AreEqual(6, six);
        }

        [ChangeParameter(NewParameter = 2)]
        [RecordCall]
        public int ReturnParameter(int a)
        {
            return a;
        }

        [EmptyMethodAdvice]
        public int Add(int a, int b, int c, int d, int e)
        {
            return a + b + c + d + e;
        }
    }
}