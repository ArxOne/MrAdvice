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
    [DebuggerDisplay("Property: {TargetProperty}")]
    public class PropertyInfoAdviceContext : AdviceInfoContext
    {
        /// <summary>
        /// Gets the target method.
        /// </summary>
        /// <value>
        /// The target method.
        /// </value>
        public PropertyInfo TargetProperty { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfoAdviceContext"/> class.
        /// </summary>
        /// <param name="targetProperty">The target property.</param>
        internal PropertyInfoAdviceContext(PropertyInfo targetProperty)
        {
            TargetProperty = targetProperty;
        }
    }
}