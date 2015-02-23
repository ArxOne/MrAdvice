#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest.Advices
{
    using System;
    using System.Threading;
    using ArxOne.MrAdvice.Advice;

    /// <summary>
    /// Allows to invoke a method asynchronously (here, in a background thread)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Async : Attribute, IMethodAdvice
    {
        public bool KillExisting { get; set; }

        [NonSerialized]
        private Thread _thread;

        public void Advise(MethodAdviceContext context)
        {
            if (KillExisting && _thread != null && _thread.IsAlive)
                _thread.Abort();
            _thread = new Thread(context.Proceed) { IsBackground = true, Name = context.TargetMethod.Name };
            _thread.Start();
        }
    }
}
