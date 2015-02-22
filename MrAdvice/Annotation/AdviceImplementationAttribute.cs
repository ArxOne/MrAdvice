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
    /// Marker for implementations of interfaces by advice
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AdviceImplementationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceImplementationAttribute"/> class.
        /// </summary>
        [Obsolete("Internal use only")]
        public AdviceImplementationAttribute()
        { }
    }
}
