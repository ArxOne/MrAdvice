#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using Advices;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InterfaceTest
    {
        [TestMethod]
        [TestCategory("Weaving")]
        public void WeaveInterfaceTest()
        {
            var a = new InterfaceMethodAdvice();
            var i = a.Handle<IAdvisedInterface>();
            Assert.IsNotNull(i);
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void EmptyCallTest()
        {
            var a = new InterfaceMethodAdvice();
            var i = a.Handle<IAdvisedInterface>();
            var r = i.DoSomething(1, 2);
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void OverrideReturnValueTest()
        {
            var a = new InterfaceMethodAdvice { NewReturnValue = 12 };
            var i = a.Handle<IAdvisedInterface>();
            var r = i.DoSomething(1, 2);
            Assert.AreEqual(12, r);
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void OverrideRefParameterTest()
        {
            var a = new InterfaceMethodAdvice { NewFirstParameter = 45 };
            var i = a.Handle<IAdvisedInterface>();
            int v = 0;
            i.DoSomethingWithRef(ref v);
            Assert.AreEqual(45, v);
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void OverrideOutParameterTest()
        {
            var a = new InterfaceMethodAdvice { NewFirstParameter = 99 };
            var i = a.Handle<IAdvisedInterface>();
            int v;
            i.DoSomethingWithOut(out v);
            Assert.AreEqual(99, v);
        }
        [TestMethod]
        [TestCategory("Interface")]
        public void PropertyGetTest()
        {
            var a = new InterfacePropertyAdvice { NewReturnValue = 67 };
            var i = a.Handle<IAdvisedInterface>();
            var v = i.SomeProperty;
            Assert.AreEqual(67, v);
        }
    }
}
