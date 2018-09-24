#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Pointcut
{
    using System.Collections.Generic;
    using System.Linq;
    using Annotation;
    using global::MrAdvice.Annotation;

    /// <summary>
    /// Represents a full pointcut selector, with any number of <see cref="PointcutSelectorRule"/>
    /// </summary>
    public class PointcutSelector
    {
        /// <summary>
        /// Gets the include rules.
        /// If empty, then everything matches
        /// </summary>
        /// <value>
        /// The include rules.
        /// </value>
        public List<PointcutSelectorRule> IncludeRules { get; } = new List<PointcutSelectorRule>();
        /// <summary>
        /// Gets the exclude rules.
        /// </summary>
        /// <value>
        /// The exclude rules.
        /// </value>
        public List<PointcutSelectorRule> ExcludeRules { get; } = new List<PointcutSelectorRule>();

        /// <summary>
        /// Indicates whether the specified [name, attribute] pair has to be selected for advice
        /// </summary>
        /// <param name="reflectionName">Name of the reflection.</param>
        /// <param name="visibilityScope">The attributes.</param>
        /// <param name="memberKind">Kind of the member.</param>
        /// <returns></returns>
        public bool Select(string reflectionName, VisibilityScope? visibilityScope, MemberKind? memberKind)
        {
            // first of all: inclusion
            // if no rule, or if any matches, then it's OK
            // below is the opposite :)
            if (IncludeRules.Count > 0 && !IncludeRules.Any(r => r.Select(reflectionName, visibilityScope, memberKind)))
                return false;
            // now check that no rule applies
            if (ExcludeRules.Count == 0)
                return true;
            return ExcludeRules.All(r => !r.Select(reflectionName, visibilityScope, memberKind));
        }

        /// <summary>
        /// The empty selector
        /// </summary>
        public static readonly PointcutSelector EmptySelector = new PointcutSelector();

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static PointcutSelector operator +(PointcutSelector a, PointcutSelector b)
        {
            if (a.IncludeRules.Count == 0 && a.ExcludeRules.Count == 0)
                return b;
            if (b.IncludeRules.Count == 0 && b.ExcludeRules.Count == 0)
                return a;
            var c = new PointcutSelector();
            c.IncludeRules.AddRange(a.IncludeRules);
            c.IncludeRules.AddRange(b.IncludeRules);
            c.ExcludeRules.AddRange(a.ExcludeRules);
            c.ExcludeRules.AddRange(b.ExcludeRules);
            return c;
        }
    }
}
