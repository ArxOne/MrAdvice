#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
    using Advices;
    using ArxOne.MrAdvice.Advice;
    using ExternalAdvices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InterfaceTest
    {
        public interface IExternalAdvisedInterface2 : IExternalAdvisedInterface { }

        [TestMethod]
        [TestCategory("Weaving")]
        public void WeaveExternalInterfaceTest()
        {
            var a = new InterfaceMethodAdvice();
            var i = a.Handle<IExternalAdvisedInterface>();
            Assert.IsNotNull(i);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void WeaveExternalInterface2Test()
        {
            var a = new InterfaceMethodAdvice();
            var i = a.Handle<IExternalAdvisedInterface2>();
            Assert.IsNotNull(i);
        }

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

        private TInterface Handle<TAdvice, TInterface>(TAdvice advice)
            where TAdvice : IAdvice
        {
            return advice.Handle<TInterface>();
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void IndirectWeavingTest()
        {
            var a = new InterfaceMethodAdvice { NewReturnValue = 87 };
            var i = Handle<IAdvice, IIndirectAdvisedInterface>(a);
            var r = i.DoSomething(4, 3);
            Assert.AreEqual(87, r);
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void InterfaceTypeTest()
        {
            var a = new InterfaceCheckAdvice();
            var i = a.Handle<IAdvisedInterface>();
            i.DoNothing();
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void DynamicHandleTest()
        {
            var a = new InterfaceCheckAdvice();
            var i = (IDynamicHandledInterface)a.Handle(typeof(IDynamicHandledInterface));
            i.Nop();
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void DynamicHandleFromInheritedTest()
        {
            var a = new InterfaceCheckAdvice();
            var i = (IDynamicHandledInheritedInterface)a.Handle(typeof(IDynamicHandledInheritedInterface));
            i.B();
        }

        [TestMethod]
        [TestCategory("Interface")]
        public void DynamicHandleFromBaseTest()
        {
            var a = new InterfaceCheckAdvice();
            var i = (IDynamicHandledInheritedInterface)a.Handle(typeof(IDynamicHandledInheritedInterface));
            i.A();
        }
    }
}
