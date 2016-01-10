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

    public class CheckSyncAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            var target = (AsyncTest)context.Target;
            context.Proceed();
            Assert.AreEqual(AsyncTest.FinalStep, target.AwaitStep);
        }
    }

    public class SyncAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }

    public class CheckAsyncAdvice : Attribute, IAsyncMethodAdvice
    {
        public async Task Advise(AsyncMethodAdviceContext context)
        {
            var target = (AsyncTest)context.Target;
            await context.ProceedAsync();
            Assert.AreEqual(AsyncTest.FinalStep, target.AwaitStep);
        }
    }

    public class AsyncAdvice : Attribute, IAsyncMethodAdvice
    {
        public async Task Advise(AsyncMethodAdviceContext context)
        {
            await context.ProceedAsync();
        }
    }

    [TestClass]
    public class AsyncTest
    {
        public int AwaitStep { get; set; }

        public const int FinalStep = 4;

        [CheckSyncAdvice]
        public async Task AwaitSteps()
        {
            for (int step = 1; step <= FinalStep; step++)
            {
                AwaitStep = step;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        [CheckAsyncAdvice]
        public async Task AwaitSteps2()
        {
            for (int step = 1; step <= FinalStep; step++)
            {
                AwaitStep = step;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        [SyncAdvice]
        public async Task<int> SumTo(int total)
        {
            var s = 0;
            for (int step = 1; step <= total; step++)
            {
                s += step;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            return s;
        }

        [AsyncAdvice]
        public async Task<int> SumTo2(int total)
        {
            var s = 0;
            for (int step = 1; step <= total; step++)
            {
                s += step;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            return s;
        }

        public void F1()
        { }

        public async void F2()
        { }

        [TestMethod]
        [TestCategory("Async")]
        public void VoidSyncTest()
        {
            var f1 = GetType().GetMethod("F1");
            var a1 = f1.GetCustomAttributes<AsyncStateMachineAttribute>().ToArray();
            var f2 = GetType().GetMethod("F2");
            var a2 = f2.GetCustomAttributes<AsyncStateMachineAttribute>().ToArray();
            Task.Run(AwaitSteps).Wait();
        }

        [TestMethod]
        [TestCategory("Async")]
        public void VoidAsyncTest()
        {
            Task.Run(AwaitSteps2).Wait();
        }

        [TestMethod]
        [TestCategory("Async")]
        public void IntSyncTest()
        {
            var t = Task.Run(() => SumTo(3));
            t.Wait();
            Assert.AreEqual(1 + 2 + 3, t.Result);
        }

        [TestMethod]
        [TestCategory("Async")]
        public void IntAsyncTest()
        {
            var t = Task.Run(() => SumTo2(4));
            t.Wait();
            Assert.AreEqual(1 + 2 + 3 + 4, t.Result);
        }
    }
}
