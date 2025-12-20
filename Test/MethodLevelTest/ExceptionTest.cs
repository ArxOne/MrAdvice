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
        public void AsyncAdviceTest()
        {
            Assert.Throws<ApplicationException>(() => { throw new ApplicationException("Something Happened!"); });
        }

        [TestMethod]
        [EmptyReallyAsyncAdvice]
        public void ReallyAsyncAdviceTest()
        {
            Assert.Throws<ApplicationException>(() => { throw new ApplicationException("Something Happened!"); });
        }
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
