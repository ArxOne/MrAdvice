#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using ArxOne.MrAdvice.Advice.Builder;
using ArxOne.MrAdvice.Introduction;

namespace MethodLevelTest
{
    using System;
    using System.Reflection;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WeavingAdviceTest
    {
        public class MethodWeavingAdvice : Attribute, IMethodWeavingAdvice
        {
            public void Advise(MethodWeavingContext context)
            {
                context.TypeWeaver.AddAutoProperty(context.TargetMethodName + "_Friend", typeof(string), MethodAttributes.Public);
                context.TargetMethodName += "_Renamed";
                context.TypeWeaver.AfterConstructors(Initializer);
                context.TypeWeaver.AfterConstructors(Initializer);
            }

            public static void Initializer(object target)
            {
                var property = target.GetType().GetProperty("WeavingAdvisedMethod_Friend");
                var currentValue = (string)property.GetValue(target, Array.Empty<object>()) ?? "";
                property.SetValue(target, currentValue + "Hello", Array.Empty<object>());
            }
        }

        public class A
        {
            private int z = 0;

            ~A()
            {
                try
                {
                    z++;
                }
                finally
                {
                    z++;
                }
            }
        }

        public class B
        {
            private int y = 0;

            ~B()
            {
                y++;
            }
        }

        public class WeavingAdvisedMethods
        {
            //public string CompilerAutoProperty { get; set; }

            [MethodWeavingAdvice]
            public void WeavingAdvisedMethod()
            {
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.IsTrue(thisMethod.Name.StartsWith("WeavingAdvisedMethod_Renamed"));
                var newProperty = GetType().GetProperty("WeavingAdvisedMethod_Friend");
                Assert.IsNotNull(newProperty);
                var newPropertyValue = (string)newProperty.GetValue(this, Array.Empty<object>());
                Assert.AreEqual("Hello", newPropertyValue);
            }

            //~WeavingAdvisedMethods() { }
        }

        [TestMethod]
        [TestCategory("Weaving advice")]
        public void SimpleWeavingAdviceTest()
        {
            var c = new WeavingAdvisedMethods();
            c.WeavingAdvisedMethod();
        }

        private interface ICount
        {
            int Constructors { get; set; }
            int Methods { get; set; }
        }

        public class TypeWeavingAdvice : Attribute, ITypeWeavingAdvice
        {
            public void Advise(WeavingContext context)
            {
                context.TypeWeaver.AfterConstructors(CountConstructors);
                context.TypeWeaver.AfterMethod("F", CountMethods);
            }

            public static void CountConstructors(object o)
            {
                ((ICount)o).Constructors++;
            }

            public static void CountMethods(object o)
            {
                ((ICount)o).Methods++;
            }
        }

        [TypeWeavingAdvice]
        public class WeavingAdvisedType : ICount
        {
            public int Constructors { get; set; }
            public int Methods { get; set; }

            public WeavingAdvisedType()
            {
            }

            public void F()
            { }

            public int F(int b)
            {
                return b + 1;
            }

            public void G()
            { }
        }

        [TestMethod]
        [TestCategory("Weaving advice")]
        public void SimpleTypeWeavingAdviceTest()
        {
            var x = new WeavingAdvisedType();
            x.F();
            _ = x.F(1);
            x.G();
            Assert.AreEqual(1, x.Constructors);
            Assert.AreEqual(2, x.Methods);
        }
    }
}
