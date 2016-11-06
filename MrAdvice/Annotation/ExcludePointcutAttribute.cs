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
    public sealed class ExcludePointcutAttribute : PointcutAttribute
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="PointcutAttribute" /> is include.
        /// </summary>
        /// <value>
        ///   <c>true</c> if include; otherwise, <c>false</c>.
        /// </value>
        public override bool Include => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludePointcutAttribute"/> class.
        /// </summary>
        public ExcludePointcutAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludePointcutAttribute"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public ExcludePointcutAttribute(params string[] names)
            : base(names)
        { }
    }
}