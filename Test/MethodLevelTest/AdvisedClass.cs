#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using System;
    using System.Threading;
    using Advices;

    public class AdvisedClass
    {
        public bool LaunchAsyncMethod()
        {
            var start = new ManualResetEvent(false);
            var end = new ManualResetEvent(false);
            AsyncMethod(start, end);
            start.Set();
            return end.WaitOne(TimeSpan.FromSeconds(10));
        }

        [Async]
        private void AsyncMethod(EventWaitHandle start, EventWaitHandle end)
        {
            if (!start.WaitOne(TimeSpan.FromSeconds(10)))
                return;
            end.Set();
        }

        [AddC, AddD, AddB, AddA, AddE]
        public string GetString(string s)
        {
            return s;
        }

        [AddC2, AddD2, AddB2, AddA2, AddE2]
        public string GetString2(string s)
        {
            return s;
        }

        [EmptyMethodAdvice]
        public void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }
    }
}
