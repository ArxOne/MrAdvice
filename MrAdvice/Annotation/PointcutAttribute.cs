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
    /// Allows to include or exclude namespaces/types/methods/etc. from being advised
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class PointcutAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name matching patterns.
        /// Default is extended Wildcard, Regex mode is enabled by using ^ at start or $ at end
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string[] Names { get; set; }

        /// <summary>
        /// Gets or sets the attributes to match.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public MemberAttributes Attributes { get; set; } = MemberAttributes.Any;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointcutAttribute"/> class.
        /// </summary>
        protected PointcutAttribute()
            : this(new string[0])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointcutAttribute"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        protected PointcutAttribute(params string[] names)
        {
            Names = names;
        }
    }
}
