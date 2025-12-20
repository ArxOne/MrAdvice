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
    using Advices;
    using NUnit.Framework;

    public class EmptyAdvisedClass
    {
        [EmptyMethodAdvice]
        public void MethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.That(thisMethod.Name, Is.Not.EqualTo("MethodTest"));
        }

        [EmptyMethodAdvice2]
        public void IndirectMethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.That(thisMethod.Name, Is.Not.EqualTo("IndirectMethodTest"));
        }

        [EmptyMethodAdvice]
        public static void StaticMethodTest()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.That(thisMethod.Name, Is.Not.EqualTo("StaticMethodTest"));
        }

        [EmptyMethodAdvice]
        public void MethodWithParameterTest(int two)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.That(thisMethod.Name, Is.Not.EqualTo("MethodWithParameterTest"));
            Assert.That(two, Is.EqualTo(2));
        }

        [EmptyMethodAdvice]
        public static void StaticMethodWithParameterTest(int three)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.That(thisMethod.Name, Is.Not.EqualTo("StaticMethodWithParameterTest"));
            Assert.That(three, Is.EqualTo(3));
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
                Assert.That(thisMethod.Name, Is.Not.EqualTo("get_Property"));
                return _property;
            }
            set
            {
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.That(thisMethod.Name, Is.Not.EqualTo("set_Property"));
                _property = value;
            }
        }

        [EmptyPropertyAdvice]
        public int Property2
        {
            get
            {
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.That(thisMethod.Name, Is.Not.EqualTo("get_Property2"));
                return _property;
            }
            set
            {
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.That(thisMethod.Name, Is.Not.EqualTo("set_Property2"));
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

        public string ReturnAdvisedProperty { [return: EmptyParameterAdvice] get; set; }

        public string this[[EmptyParameterAdvice] string index]
        { get { return null; } set { } }

        [EmptyMethodAdvice]
        public void MethodWithGenericParameterTest<TValue>(TValue six)
        {
            Assert.That(Int32.Parse(six.ToString()), Is.EqualTo(6));
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