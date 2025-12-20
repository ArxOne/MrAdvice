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
    using NUnit.Framework;

    [TestFixture]
    [Category("Exceptions")]
    public class ExceptionTest
    {
        [Test]
        [EmptyAsyncAdvice]
        public void AsyncAdviceTest()
        {
            Assert.That(() => { throw new ApplicationException("Something Happened!"); },
                Throws.TypeOf<ApplicationException>());
        }

        [Test]
        [EmptyReallyAsyncAdvice]
        public void ReallyAsyncAdviceTest()
        {
            Assert.That(() => { throw new ApplicationException("Something Happened!"); },
                Throws.TypeOf<ApplicationException>());
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