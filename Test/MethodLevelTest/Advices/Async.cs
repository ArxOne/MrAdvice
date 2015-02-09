
namespace BlueDwarf.Utility
{
    using System;
    using System.Reflection;
    using System.Threading;
    using ArxOne.Weavisor.Advice;

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
