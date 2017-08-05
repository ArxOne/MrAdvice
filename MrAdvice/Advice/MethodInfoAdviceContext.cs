#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// Info context for MethodBase
    /// </summary>
    [DebuggerDisplay("MethodInfo {" + nameof(TargetMethod) + "}")]
    public class MethodInfoAdviceContext : AdviceInfoContext
    {
        /// <summary>
        /// Gets the target method.
        /// </summary>
        /// <value>
        /// The target method.
        /// </value>
        public MethodBase TargetMethod { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodInfoAdviceContext"/> class.
        /// </summary>
        /// <param name="targetMethod">The target method.</param>
        internal MethodInfoAdviceContext(MethodBase targetMethod)
        {
            TargetMethod = targetMethod;
        }
    }
}
