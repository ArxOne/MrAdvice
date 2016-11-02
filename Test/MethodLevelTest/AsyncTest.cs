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

    public class CustomException : Exception
    {
    }

    public class CustomException2 : Exception
    {
    }

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

    public class CheckAsyncAdvice : Attribute, IMethodAsyncAdvice
    {
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            var target = (AsyncTest)context.Target;
            await context.ProceedAsync();
            Assert.AreEqual(AsyncTest.FinalStep, target.AwaitStep);
        }
    }

    public class AsyncAdvice : Attribute, IMethodAsyncAdvice
    {
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            await context.ProceedAsync();
        }
    }

    public class AsyncExceptionTranslationAdvice : Attribute, IMethodAsyncAdvice
    {
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            try
            {
                await context.ProceedAsync();
            }
            catch (CustomException)
            {
                throw new CustomException2();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncPlusOne : Attribute, IMethodAsyncAdvice
    {
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            await context.ProceedAsync();
            context.ReturnValue = Plus(((dynamic)context.ReturnValue).Result, 1);
        }

        private async Task<int> Plus(int i, int j)
        {
            return await Task.FromResult(i + j);
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

        [AsyncAdvice]
        public async Task ThrowException(bool now)
        {
            if (!now)
                await Task.Delay(TimeSpan.FromSeconds(2));
            throw new CustomException();
        }

        [AsyncExceptionTranslationAdvice]
        public async Task ThrowTranslatedException(bool now)
        {
            if (!now)
                await Task.Delay(TimeSpan.FromSeconds(2));
            throw new CustomException();
        }

        public async Task RawThrowException(bool now)
        {
            if (!now)
                await Task.Delay(TimeSpan.FromSeconds(2));
            throw new CustomException();
        }

        [AsyncAdvice]
        public int RegularSumTo(int total)
        {
            return Enumerable.Range(1, total).Sum();
        }

        public void F1()
        { }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async void F2()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        { }

        //[TestMethod]
        //[TestCategory("Async")]
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

        [TestMethod]
        [TestCategory("Async")]
        public void AsyncOnSyncTest()
        {
            var t = RegularSumTo(5);
            Assert.AreEqual(1 + 2 + 3 + 4 + 5, t);
        }

        [TestMethod]
        [TestCategory("Async")]
        [ExpectedException(typeof(CustomException))]
        public void ImmediateExceptionTest()
        {
            try
            {
                var t = Task.Run(() => ThrowException(true));
                t.Wait();
            }
            catch (AggregateException e) when (e.InnerException is CustomException)
            {
                throw e.InnerException;
            }
        }

        [TestMethod]
        [TestCategory("Async")]
        [ExpectedException(typeof(CustomException))]
        public void DelayedExceptionTest()
        {
            try
            {
                var t = Task.Run(() => ThrowException(false));
                t.Wait();
            }
            catch (AggregateException e) when (e.InnerException is CustomException)
            {
                throw e.InnerException;
            }
        }

        [TestMethod]
        [TestCategory("Async")]
        [ExpectedException(typeof(CustomException2))]
        public void DelayedTranslatedExceptionTest()
        {
            try
            {
                var t = Task.Run(() => ThrowTranslatedException(false));
                t.Wait();
            }
            catch (AggregateException e) when (e.InnerException is CustomException2)
            {
                throw e.InnerException;
            }
        }

        [TestMethod]
        [TestCategory("Async")]
        [ExpectedException(typeof(CustomException))]
        public void NotAdvisedExceptionTest()
        {
            try
            {
                var t = Task.Run(() => RawThrowException(false));
                t.Wait();
            }
            catch (AggregateException e) when (e.InnerException is CustomException)
            {
                throw e.InnerException;
            }
        }

        [AsyncPlusOne]
        public async Task<int> Get100()
        {
            await Task.Delay(3000);
            return 100;
        }

        [TestMethod]
        [TestCategory("Async")]
        public void PlusOneTest()
        {
            var t = Task.Run(Get100);
            t.Wait();
            var r = t.Result;
            Assert.AreEqual(101, r);
        }

        public class AsyncPlusOne2 : Attribute, IMethodAsyncAdvice
        {
            public async Task Advise(MethodAsyncAdviceContext context)
            {
                await context.ProceedAsync();
                if (context.HasReturnValue)
                {
                    if (context.ReturnValue is Task)
                        context.ReturnValue = Plus(((dynamic)context.ReturnValue).Result, 1);
                    else
                        context.ReturnValue = (int)context.ReturnValue + 1;
                }
            }

            private async Task<int> Plus(int i, int j)
            {
                await Task.Delay(1000);
                return await Task.FromResult(i + j);
            }
        }
        [AsyncPlusOne2]
        public int Get1000()
        {
            //Thread.Sleep(3000);
            return 1000;
        }

        [TestMethod]
        [TestCategory("Async")]
        public void AsyncOnSync2Test()
        {
            var r = Get1000();
            Assert.AreEqual(1001, r);
        }

        [AsyncAdvice]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async void GenerateWarning()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        { }

#if did_not_make_the_point
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
        public class LogSuccessAsyncAttribute : Attribute, IMethodAsyncAdvice
        {
            public async Task Advise(MethodAsyncAdviceContext context)
            {
                await context.ProceedAsync();
                Console.WriteLine("Success!");
            }
        }

        public class Class
        {
            [LogSuccessAsync]
            public async Task<int> Method()
            {
                await Task.Yield();
                throw new NullReferenceException();
            }
        }

        [TestMethod]
        [TestCategory("Async")]
        public async Task ShouldTest()
        {
            await Task.Yield();
            var c = new Class();
            Action d = async () => await c.Method();
            d();
            //  Assert.Throws(async () => await c.Method());
        }
#endif

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
        public class LogSuccessAsyncAttribute : Attribute, IMethodAsyncAdvice
        {
            public LogSuccessAsyncAttribute() { }

            public async Task Advise(MethodAsyncAdviceContext context)
            {
                await context.ProceedAsync();
                Console.WriteLine("Success!");
            }
        }

        public class Class
        {
            [LogSuccessAsync]
            public Task<int> Method2(object p)
            {
                if (p == null)
                {
                    throw new ArgumentNullException("p");
                }
                return Task.FromResult(0);
            }
        }

        [TestMethod]
        [TestCategory("Async")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AsyncRunTest()
        {
            var c = new Class();
            await c.Method2(null);
        }
    }
}
