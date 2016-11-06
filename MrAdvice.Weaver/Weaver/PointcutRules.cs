#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System.Collections.Generic;
    using System.Linq;
    using Reflection.Groups;

    public class PointcutRules
    {
        public List<PointcutRule> IncludeRules { get; } = new List<PointcutRule>();
        public List<PointcutRule> ExcludeRules { get; } = new List<PointcutRule>();

        public bool Match(ReflectionNode node)
        {
            // first of all: inclusion
            // if no rule, or if any matches, then it's OK
            // below is the opposite :)
            if (IncludeRules.Count > 0 && !IncludeRules.Any(r => r.Match(node)))
                return false;
            // now check that no rule applies
            return ExcludeRules.All(r => !r.Match(node));
        }

        public static PointcutRules NoRules = new PointcutRules();

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static PointcutRules operator +(PointcutRules a, PointcutRules b)
        {
            if (a.IncludeRules.Count == 0 && a.ExcludeRules.Count == 0)
                return b;
            if (b.IncludeRules.Count == 0 && b.ExcludeRules.Count == 0)
                return a;
            var c = new PointcutRules();
            c.IncludeRules.AddRange(a.IncludeRules);
            c.IncludeRules.AddRange(b.IncludeRules);
            c.ExcludeRules.AddRange(a.ExcludeRules);
            c.ExcludeRules.AddRange(b.ExcludeRules);
            return c;
        }
    }
}
