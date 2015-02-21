#region Weavisor
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Aspect
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Aspect, with pointcut and advices applied to it
    /// </summary>
    internal class AspectInfo
    {
        /// <summary>
        /// Gets the advices applied to pointcut in this aspect.
        /// </summary>
        /// <value>
        /// The advices.
        /// </value>
        public IList<AdviceInfo> Advices { get; set; }
        /// <summary>
        /// Gets the pointcut method.
        /// </summary>
        /// <value>
        /// The pointcut method.
        /// </value>
        public MethodInfo PointcutMethod { get; set; }
        /// <summary>
        /// Gets the pointcut property, if any (if method is related to property).
        /// </summary>
        /// <value>
        /// The pointcut property.
        /// </value>
        public PropertyInfo PointcutProperty { get; set; }
        /// <summary>
        /// Gets a value indicating whether this instance is pointcut property setter.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is pointcut property setter; otherwise, <c>false</c>.
        /// </value>
        public bool IsPointcutPropertySetter { get; set; }
    }
}