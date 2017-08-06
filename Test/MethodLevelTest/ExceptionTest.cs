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

    [TestClass]
    public class ExceptionTest
    {
        [TestMethod]
        [EmptyAsyncAdvice]
        [ExpectedException(typeof(ApplicationException))]
        public void AsyncAdviceTest() => throw new ApplicationException("Something Happened!");

        [TestMethod]
        [EmptyReallyAsyncAdvice]
        [ExpectedException(typeof(AggregateException))]
        public void ReallyAsyncAdviceTest() => throw new ApplicationException("Something Happened!");
    }

    public class EmptyAsyncAdviceAttribute : Attribute, IMethodAsyncAdvice
    {
        public Task Advise(MethodAsyncAdviceContext context)
        {
            return context.ProceedAsync();
        }
    }

    public class EmptyReallyAsyncAdviceAttribute : Attribute, IMethodAsyncAdvice
    {
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            await context.ProceedAsync();
        }
    }
}
