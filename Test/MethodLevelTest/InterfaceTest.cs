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
    using NUnit.Framework;

    [TestFixture]
    [Category("Interface")]
    public class InterfaceTest
    {
        public interface IExternalAdvisedInterface2 : IExternalAdvisedInterface { }

        [Test]
        [Category("Weaving")]
        public void WeaveExternalInterfaceTest()
        {
            var a = new InterfaceMethodAdvice();
            var i = a.Handle<IExternalAdvisedInterface>();
            Assert.That(i, Is.Not.Null);
        }

        [Test]
        [Category("Weaving")]
        public void WeaveExternalInterface2Test()
        {
            var a = new InterfaceMethodAdvice();
            var i = a.Handle<IExternalAdvisedInterface2>();
            Assert.That(i, Is.Not.Null);
        }

        [Test]
        [Category("Weaving")]
        public void WeaveInterfaceTest()
        {
            var a = new InterfaceMethodAdvice();
            var i = a.Handle<IAdvisedInterface>();
            Assert.That(i, Is.Not.Null);
        }

        [Test]
        public void EmptyCallTest()
        {
            var a = new InterfaceMethodAdvice();
            var i = a.Handle<IAdvisedInterface>();
            var r = i.DoSomething(1, 2);
            // Pas d'assertion dans l'original, on vérifie juste que l'appel ne crash pas
        }

        [Test]
        public void OverrideReturnValueTest()
        {
            var a = new InterfaceMethodAdvice { NewReturnValue = 12 };
            var i = a.Handle<IAdvisedInterface>();
            var r = i.DoSomething(1, 2);
            Assert.That(r, Is.EqualTo(12));
        }

        [Test]
        public void OverrideRefParameterTest()
        {
            var a = new InterfaceMethodAdvice { NewFirstParameter = 45 };
            var i = a.Handle<IAdvisedInterface>();
            int v = 0;
            i.DoSomethingWithRef(ref v);
            Assert.That(v, Is.EqualTo(45));
        }

        [Test]
        public void OverrideOutParameterTest()
        {
            var a = new InterfaceMethodAdvice { NewFirstParameter = 99 };
            var i = a.Handle<IAdvisedInterface>();
            int v;
            i.DoSomethingWithOut(out v);
            Assert.That(v, Is.EqualTo(99));
        }

        [Test]
        public void PropertyGetTest()
        {
            var a = new InterfacePropertyAdvice { NewReturnValue = 67 };
            var i = a.Handle<IAdvisedInterface>();
            var v = i.SomeProperty;
            Assert.That(v, Is.EqualTo(67));
        }

        private TInterface Handle<TAdvice, TInterface>(TAdvice advice)
            where TAdvice : IAdvice
        {
            return advice.Handle<TInterface>();
        }

        [Test]
        public void IndirectWeavingTest()
        {
            var a = new InterfaceMethodAdvice { NewReturnValue = 87 };
            var i = Handle<IAdvice, IIndirectAdvisedInterface>(a);
            var r = i.DoSomething(4, 3);
            Assert.That(r, Is.EqualTo(87));
        }

        [Test]
        public void InterfaceTypeTest()
        {
            var a = new InterfaceCheckAdvice();
            var i = a.Handle<IAdvisedInterface>();
            i.DoNothing();
        }

        [Test]
        public void DynamicHandleTest()
        {
            var a = new InterfaceCheckAdvice();
            var i = (IDynamicHandledInterface)a.Handle(typeof(IDynamicHandledInterface));
            i.Nop();
        }

        [Test]
        public void DynamicHandleFromInheritedTest()
        {
            var a = new InterfaceCheckAdvice();
            var i = (IDynamicHandledInheritedInterface)a.Handle(typeof(IDynamicHandledInheritedInterface));
            i.B();
        }

        [Test]
        public void DynamicHandleFromBaseTest()
        {
            var a = new InterfaceCheckAdvice();
            var i = (IDynamicHandledInheritedInterface)a.Handle(typeof(IDynamicHandledInheritedInterface));
            i.A();
        }
    }
}