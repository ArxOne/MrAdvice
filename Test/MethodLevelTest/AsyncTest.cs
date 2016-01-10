#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class AsyncAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            var target = (AsyncTest)context.Target;
            context.Proceed();
            Assert.AreEqual(AsyncTest.FinalStep, target.AwaitStep);
        }
    }

    [TestClass]
    public class AsyncTest
    {
        public int AwaitStep { get; set; }

        public const int FinalStep = 4;

        [AsyncAdvice]
        public async Task AwaitSteps()
        {
            for (int step = 1; step < FinalStep; step++)
            {
                AwaitStep = step;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        public void F1()
        { }

        public async void F2()
        { }

        [TestMethod]
        [TestCategory("Async")]
        public void SimpleAsyncTest()
        {
            var f1 = GetType().GetMethod("F1");
            var a1 = f1.GetCustomAttributes<AsyncStateMachineAttribute>().ToArray();
            var f2 = GetType().GetMethod("F2");
            var a2 = f2.GetCustomAttributes<AsyncStateMachineAttribute>().ToArray();
            Task.Run(AwaitSteps).Wait();
        }
    }
}
