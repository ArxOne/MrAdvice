#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
    using System;

    /// <summary>
    /// Context for <see cref="IMethodWeavingAdvice"/>
    /// </summary>
    public abstract class MethodWeavingContext : WeavingContext
    {
        /// <summary>
        /// Gets or sets the name of the target method.
        /// </summary>
        /// <value>
        /// The name of the target method.
        /// </value>
        public string TargetMethodName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodWeavingContext" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="targetMethodName">Name of the target method.</param>
        protected MethodWeavingContext(Type type, string targetMethodName)
            : base(type)
        {
            TargetMethodName = targetMethodName;
        }
    }
}
