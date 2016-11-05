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
    using System.Text.RegularExpressions;
    using Annotation;
    using Reflection.Groups;

    public class PointcutRule
    {
        /// <summary>
        /// Gets the names.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        public List<string> Names { get; } = new List<string>();

        public MemberAttributes Attributes { get; set; } = MemberAttributes.AnyVisiblity;

        /// <summary>
        /// Tells if the given string matches the given wildcard.
        /// Two wildcards are allowed: '*' and '?'
        /// '*' matches 0 or more characters
        /// '?' matches any character
        /// </summary>
        /// <param name="wildcard">The wildcard.</param>
        /// <param name="s">The s.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static bool WildcardMatch(string wildcard, string s, bool ignoreCase = false)
        {
            return WildcardMatch(wildcard, s, 0, 0, ignoreCase);
        }

        /// <summary>
        /// Internal matching algorithm.
        /// </summary>
        /// <param name="wildcard">The wildcard.</param>
        /// <param name="s">The s.</param>
        /// <param name="wildcardIndex">Index of the wildcard.</param>
        /// <param name="sIndex">Index of the s.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        private static bool WildcardMatch(string wildcard, string s, int wildcardIndex, int sIndex, bool ignoreCase)
        {
            for (;;)
            {
                // in the wildcard end, if we are at tested string end, then strings match
                if (wildcardIndex == wildcard.Length)
                    return sIndex == s.Length;

                var c = wildcard[wildcardIndex];
                switch (c)
                {
                    // always a match
                    case '?':
                        break;
                    case '!': // everything but dot matches
                        if (s[sIndex] == '.')
                            return false;
                        break;
                    case '*':
                        // if this is the last wildcard char, then we have a match, whatever the tested string is
                        if (wildcardIndex == wildcard.Length - 1)
                            return true;
                        // test if a match follows
                        return Enumerable.Range(sIndex, s.Length - 1).Any(i => WildcardMatch(wildcard, s, wildcardIndex + 1, i, ignoreCase));
                    case '@':
                        // test if a match follows
                        var nextDotIndex = s.IndexOf('.', sIndex);
                        if (nextDotIndex < 0)
                            nextDotIndex = s.Length - 1;
                        return Enumerable.Range(sIndex, nextDotIndex).Any(i => WildcardMatch(wildcard, s, wildcardIndex + 1, i, ignoreCase));
                    default:
                        var cc = ignoreCase ? char.ToLower(c) : c;
                        var sc = ignoreCase ? char.ToLower(s[sIndex]) : s[sIndex];
                        if (cc != sc)
                            return false;
                        break;
                }

                wildcardIndex++;
                sIndex++;
            }
        }

        public static bool Match(string rule, string value)
        {
            if (rule.StartsWith("^") || rule.EndsWith("$"))
            {
                var match = Regex.Match(value, rule);
                return match.Success;
            }
            return WildcardMatch(rule, value);
        }

        public static bool MatchAttributes(MemberAttributes memberAttributes, ReflectionNode node)
        {
            var nodeAttributes = node.Attributes;
            if (!nodeAttributes.HasValue)
                return true;
            return (nodeAttributes.Value & memberAttributes) != 0;
        }

        public bool MatchAttributes(ReflectionNode node) => MatchAttributes(Attributes, node);

        public bool MatchName(string rule, ReflectionNode node)
        {
            var nodeName = node.Name;
            if (nodeName == null)
                return true;
            return Match(rule, nodeName);
        }

        public bool MatchName(ReflectionNode node) => Names.Any(n => MatchName(n, node));

        /// <summary>
        /// Indicates whether this rule matches the given node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public bool Match(ReflectionNode node) => MatchAttributes(node) && MatchName(node);
    }
}
