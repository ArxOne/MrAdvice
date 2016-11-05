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
    }
}
