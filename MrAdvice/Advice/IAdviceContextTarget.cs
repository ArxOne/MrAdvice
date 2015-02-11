#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Target part with advice context
    /// Used by introduced fields
    /// </summary>
    public interface IAdviceContextTarget
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        object Target { get; }
    }
}
