
namespace MethodLevelTest
{
    using System;
    using System.Threading;
    using BlueDwarf.Utility;

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
    }
}
