#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    /// <summary>
    /// Exclusion filters for pointcuts.
    /// This has to be applied on advices
    /// </summary>
    /// <seealso cref="PointcutAttribute" />
    public sealed class IncludePointcutAttribute : PointcutAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncludePointcutAttribute"/> class.
        /// </summary>
        public IncludePointcutAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludePointcutAttribute"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public IncludePointcutAttribute(params string[] names)
            : base(names)
        { }
    }
}