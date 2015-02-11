#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Base class for advice information context
    /// </summary>
    public class AdviceInfoContext : IAdviceContextTarget
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public object Target
        {
            get { return null; }
        }
    }
}
