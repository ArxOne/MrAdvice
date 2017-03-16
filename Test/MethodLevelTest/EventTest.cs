#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace MethodLevelTest
{
    using System;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class EventBlockerAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            //context.Proceed();
        }
    }
    
    public class EventClass
    {
        [EventBlockerAdvice]
        public event EventHandler SimpleEvent;

        public void OnSimpleEvent()
        {
            SimpleEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    [TestClass]
    public class EventTest
    {
        [TestMethod]
        public void AdviseEventTest()
        {
            bool wasInvoked = false;
            var ec = new EventClass();
            ec.SimpleEvent += delegate { wasInvoked = true; };

            ec.OnSimpleEvent();
            Assert.IsFalse(wasInvoked);
        }
    }
}