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
    /// Allows a class to be advice-proof, by specifying which advices won't apply
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class ExcludeAdvicesAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name matching patterns.
        /// Default is extended Wildcard, Regex mode is enabled by using ^ at start or $ at end
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string[] AdvicesTypes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeAdvicesAttribute"/> class.
        /// </summary>
        public ExcludeAdvicesAttribute()
            : this(new string[0])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeAdvicesAttribute" /> class.
        /// </summary>
        /// <param name="advicesTypes">The types.</param>
        public ExcludeAdvicesAttribute(params string[] advicesTypes)
        {
            AdvicesTypes = advicesTypes;
        }
    }
}