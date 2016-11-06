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
    using System.Linq.Expressions;
    using Annotation;
    using dnlib.DotNet;
    using Utility;

    partial class AspectWeaver
    {
        /// <summary>
        /// Gets the rules at given <see cref="MarkedNode"/>.
        /// </summary>
        /// <param name="markedNode">The marked node.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutRules GetPointcutRules(MarkedNode markedNode, WeavingContext context)
        {
            var rules = PointcutRules.NoRules;
            foreach (var markerDefinition in markedNode.Definitions)
                rules += GetPointcutRules(markerDefinition.Type, context);
            return rules;
        }

        /// <summary>
        /// Gets the rules at given advice.
        /// </summary>
        /// <param name="adviceType">Type of the advice.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutRules GetPointcutRules(ITypeDefOrRef adviceType, WeavingContext context)
        {
            PointcutRules pointcutRules;
            if (context.AdvicesRules.TryGetValue(adviceType, out pointcutRules))
                return pointcutRules;
            context.AdvicesRules[adviceType] = pointcutRules = CreatePointcutRules(adviceType, context);
            return pointcutRules;
        }

        private PointcutRules CreatePointcutRules(ITypeDefOrRef adviceType, WeavingContext context)
        {
            var adviceTypeDef = TypeResolver.Resolve(adviceType);
            var rules = new PointcutRules();
            foreach (var customAttribute in adviceTypeDef.CustomAttributes)
                rules += CreatePointcutRules(customAttribute, context);
            return rules;
        }

        private PointcutRules CreatePointcutRules(CustomAttribute customAttribute, WeavingContext context)
        {
            var rules = new PointcutRules();
            rules.IncludeRules.AddRange(CreatePointcutRule(customAttribute, context.IncludePointcutAttributeType));
            rules.ExcludeRules.AddRange(CreatePointcutRule(customAttribute, context.ExcludePointcutAttributeType));
            return rules;
        }

        private IEnumerable<PointcutRule> CreatePointcutRule(CustomAttribute customAttribute, ITypeDefOrRef pointcutAttributeType)
        {
            if (customAttribute.AttributeType.SafeEquivalent(pointcutAttributeType))
            {
                var rule = new PointcutRule();
                // full names wildcards
                if (customAttribute.ConstructorArguments.Count == 1)
                {
                    rule.Names.AddRange(((IEnumerable<CAArgument>)customAttribute.ConstructorArguments[0].Value).Select(a => (string)(UTF8String)a.Value));
                }
                foreach (var namedArgument in customAttribute.NamedArguments)
                {
                    if (namedArgument.Name == nameof(PointcutAttribute.Names))
                    {
                        rule.Names.AddRange(((IEnumerable<CAArgument>)namedArgument.Value).Select(a => (string)(UTF8String)a.Value));
                    }
                    if (namedArgument.Name == nameof(PointcutAttribute.Attributes))
                    {
                        rule.Attributes = (MemberAttributes)namedArgument.Value;
                    }
                }
                yield return rule;
            }
        }
    }
}
