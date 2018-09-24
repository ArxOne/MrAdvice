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
    using System.Text.RegularExpressions;
    using Annotation;
    using global::MrAdvice.Annotation;

    /// <summary>
    /// Represents a simple rule of pointcut
    /// It contains: 
    /// - one or more name matching rules
    /// - an attribute matching rule
    /// </summary>
    public class PointcutSelectorRule
    {
        /// <summary>
        /// Gets the names.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        public List<string> Names { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the attributes to match.
        /// </summary>
        /// <value>
        /// The attributes.
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
        /// Initializes a new instance of the <see cref="PointcutSelectorRule"/> class.
        /// </summary>
        public PointcutSelectorRule() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointcutSelectorRule"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public PointcutSelectorRule(params string[] names)
        {
            Names.AddRange(names);
        }

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
            for (; ; )
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
                        if (sIndex >= s.Length)
                            return false;
                        var sc = ignoreCase ? char.ToLower(s[sIndex]) : s[sIndex];
                        if (cc != sc)
                            return false;
                        break;
                }

                wildcardIndex++;
                sIndex++;
            }
        }

        /// <summary>
        /// Indicates whether the specified value matches the given rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool Match(string rule, string value)
        {
            if (rule.StartsWith("^") || rule.EndsWith("$"))
            {
                var match = Regex.Match(value, rule);
                return match.Success;
            }
            return WildcardMatch(rule, value);
        }

        /// <summary>
        /// Matches the name given the rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="reflectionName">Name of the reflection.</param>
        /// <returns></returns>
        private bool MatchName(string rule, string reflectionName)
        {
            if (reflectionName == null)
                return true;
            return Match(rule, reflectionName);
        }

        /// <summary>
        /// Indicates whether the name must be selected for advice.
        /// </summary>
        /// <param name="reflectionName">Name of the reflection.</param>
        /// <returns></returns>
        public bool Select(string reflectionName)
        {
            if (Names.Count == 0)
                return true;
            return Names.Any(n => MatchName(n, reflectionName));
        }

        /// <summary>
        /// Indicates whether the scope must be selected for advice.
        /// </summary>
        /// <param name="visibilityScope">The member attributes.</param>
        /// <returns></returns>
        public bool Select(VisibilityScope? visibilityScope)
        {
            if (!visibilityScope.HasValue)
                return true;
            return (visibilityScope.Value & Scope) != 0;
        }

        /// <summary>
        /// Indicates whether the kind must be selected for advice.
        /// </summary>
        /// <param name="memberKind">Kind of the member.</param>
        /// <returns></returns>
        public bool Select(MemberKind? memberKind)
        {
            if (!memberKind.HasValue)
                return true;
            return (memberKind.Value & Kind) != 0;
        }

        /// <summary>
        /// Indicates if the pair [name, attribute] have to be selected for advice
        /// </summary>
        /// <param name="reflectionName">Name of the reflection.</param>
        /// <param name="visibilityScope">The member attributes.</param>
        /// <param name="memberKind">Kind of the member.</param>
        /// <returns></returns>
        public bool Select(string reflectionName, VisibilityScope? visibilityScope, MemberKind? memberKind)
        {
            return Select(visibilityScope) && Select(memberKind) && Select(reflectionName);
        }
    }
}
