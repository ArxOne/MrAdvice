#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Base class for interface auto implementations
    /// </summary>
    public class AdvisedInterface
    {
        /// <summary>
        /// The injected advice
        /// </summary>
        internal IAdvice Advice;
    }
}
