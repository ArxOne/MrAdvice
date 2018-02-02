#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
    using System.Diagnostics;

    /// <summary>
    /// Sync advice context
    /// </summary>
    /// <seealso cref="ArxOne.MrAdvice.Advice.AdviceContext" />
    public abstract class SyncAdviceContext : AdviceContext
    {
        internal SyncAdviceContext(AdviceValues adviceValues, AdviceContext nextAdviceContext)
            : base(adviceValues, nextAdviceContext)
        {
        }

        /// <summary>
        /// Proceeds to the next advice
        /// </summary>
        /// <remarks>On async methods, this method may return before the task completes. To wait for full completion, 
        /// implement <see cref="IMethodAsyncAdvice"/> and use ProceedAsync() method</remarks>
        [DebuggerStepThrough]
        public virtual void Proceed() => InvokeNext();
    }
}