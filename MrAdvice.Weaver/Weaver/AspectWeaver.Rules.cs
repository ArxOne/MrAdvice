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
    using Pointcut;
    using Utility;

    partial class AspectWeaver
    {
        /// <summary>
        /// Gets the rules at given <see cref="MarkedNode"/>.
        /// </summary>
        /// <param name="markedNode">The marked node.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutSelector GetPointcutRules(MarkedNode markedNode, WeavingContext context)
        {
            var rules = PointcutSelector.EmptySelector;
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
        private PointcutSelector GetPointcutRules(ITypeDefOrRef adviceType, WeavingContext context)
        {
            PointcutSelector pointcutRules;
            if (context.AdvicesRules.TryGetValue(adviceType, out pointcutRules))
                return pointcutRules;
            context.AdvicesRules[adviceType] = pointcutRules = CreatePointcutRules(adviceType, context);
            return pointcutRules;
        }

        /// <summary>
        /// Creates the pointcut rules for a given advice.
        /// </summary>
        /// <param name="adviceType">Type of the advice.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutSelector CreatePointcutRules(ITypeDefOrRef adviceType, WeavingContext context)
        {
            var adviceTypeDef = TypeResolver.Resolve(adviceType);
            var rules = new PointcutSelector();
            foreach (var customAttribute in adviceTypeDef.CustomAttributes)
                rules += CreatePointcutRules(customAttribute, context);
            return rules;
        }

        /// <summary>
        /// Creates the pointcut rules from an advice attribute.
        /// The custom attribute provided has to be either <see cref="ExcludePointcutAttribute"/> or <see cref="IncludePointcutAttribute"/>
        /// If not the return <see cref="PointcutSelector"/> are empty
        /// </summary>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutSelector CreatePointcutRules(CustomAttribute customAttribute, WeavingContext context)
        {
            var rules = new PointcutSelector();
            rules.IncludeRules.AddRange(CreatePointcutRule(customAttribute, context.IncludePointcutAttributeType));
            rules.ExcludeRules.AddRange(CreatePointcutRule(customAttribute, context.ExcludePointcutAttributeType));
            return rules;
        }

        /// <summary>
        /// Creates the pointcut rule from the given custom attribute of the the given type.
        /// </summary>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <param name="pointcutAttributeType">Type of the pointcut attribute.</param>
        /// <returns></returns>
        private IEnumerable<PointcutSelectorRule> CreatePointcutRule(CustomAttribute customAttribute, ITypeDefOrRef pointcutAttributeType)
        {
            if (!customAttribute.AttributeType.SafeEquivalent(pointcutAttributeType))
                yield break;

            var rule = new PointcutSelectorRule();
            // full names wildcards
            if (customAttribute.ConstructorArguments.Count == 1)
                rule.Names.AddRange(GetStrings(customAttribute.ConstructorArguments[0].Value));

            // then named properties
            foreach (var namedArgument in customAttribute.NamedArguments)
            {
                // names (which should usually not happen)
                if (namedArgument.Name == nameof(PointcutAttribute.Names))
                    rule.Names.AddRange(GetStrings(namedArgument.Value));
                // attributes
                if (namedArgument.Name == nameof(PointcutAttribute.Attributes))
                    rule.Attributes = (MemberAttributes)namedArgument.Value;
            }
            yield return rule;
        }

        /// <summary>
        /// Gets the strings from a dnlib custom attribute argument.
        /// </summary>
        /// <param name="caArgumentValue">The ca argument value.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetStrings(object caArgumentValue)
        {
            return ((IEnumerable<CAArgument>)caArgumentValue).Select(a => (string)(UTF8String)a.Value);
        }
    }
}
