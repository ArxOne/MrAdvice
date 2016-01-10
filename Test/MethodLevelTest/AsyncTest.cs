#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
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

        [TestMethod]
        [TestCategory("Async")]
        public void SimpleAsyncTest()
        {
            Task.Run(AwaitSteps).Wait();
        }
    }
}
