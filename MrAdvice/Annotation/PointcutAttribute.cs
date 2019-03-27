#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using System;
    using global::MrAdvice.Annotation;

    /// <summary>
    /// Allows to include or exclude namespaces/types/methods/etc. from being advised
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public abstract class PointcutAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name matching patterns.
        /// Wildcards are ? and * (as usual)
        /// or ! which matches any character but the "." (dot)
        /// or @ which matches any string until a "." (dot) is met
        /// Default is extended Wildcard, Regex mode is enabled by using ^ at start or $ at end
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string[] Names { get; set; }

        /// <summary>
        /// Gets or sets the scope of items where the .
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public VisibilityScope Scope { get; set; } = VisibilityScope.Any;

        /// <summary>
        /// Gets or sets the kind.
        /// </summary>
        /// <value>
        /// The kind.
        /// </value>
        public MemberKind Kind { get; set; } = MemberKind.Any;

        /// <summary>
        /// Gets or sets the attributes to match.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [Obsolete("Use Scope property instead")]
        public VisibilityScope Attributes
        {
            get => Scope;
            set => Scope = value;
        }

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
