#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Advice
{
    using System.Reflection;

    /// <summary>
    /// Info context for MethodBase
    /// </summary>
    public class MethodInfoAdviceContext : AdviceInfoContext
    {
        /// <summary>
        /// Gets the target method.
        /// </summary>
        /// <value>
        /// The target method.
        /// </value>
        public MethodBase TargetMethod { get; private set; }

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
