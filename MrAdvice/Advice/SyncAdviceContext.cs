#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
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
        public void Proceed() => InvokeNext()?.Wait();
    }
}