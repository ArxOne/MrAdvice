#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using System;

    /// <summary>
    /// An advice marked with this attribute will remove the target property/method
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AbstractTargetAttribute : PriorityAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractTargetAttribute"/> class.
        /// </summary>
        public AbstractTargetAttribute()
            : base(-1)
        {
        }
    }
}
