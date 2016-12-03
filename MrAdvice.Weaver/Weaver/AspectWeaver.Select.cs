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
    using Annotation;
    using dnlib.DotNet;
    using Pointcut;
    using Reflection.Groups;
    using Utility;

    partial class AspectWeaver
    {
        /// <summary>
        /// Gets the rules at given <see cref="MarkedNode"/>.
        /// </summary>
        /// <param name="markedNode">The marked node.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutSelector GetPointcutSelector(MarkedNode markedNode, WeavingContext context)
        {
            var rules = PointcutSelector.EmptySelector;
            foreach (var markerDefinition in markedNode.Definitions)
                rules += GetPointcutSelector(markerDefinition.Type, context);
            return rules;
        }

        private PointcutSelector GetAdviceSelector(ReflectionNode node, WeavingContext context)
        {
            if (node.AdviceSelector != null)
                return node.AdviceSelector;

            return node.AdviceSelector = CreateAdviceSelector(node, context);
        }

        private PointcutSelector CreateAdviceSelector(ReflectionNode node, WeavingContext context)
        {
            var adviceSelector = new PointcutSelector();
            // Advices should not advise themselves
            var typeReflectionNode = node as TypeReflectionNode;
            if (typeReflectionNode != null && IsMarker(typeReflectionNode.TypeDefinition, context.AdviceInterfaceType))
            {
                Logging.WriteDebug("Excluding {0} from itself", typeReflectionNode.TypeDefinition.FullName);
                adviceSelector.ExcludeRules.Add(new PointcutSelectorRule(typeReflectionNode.TypeDefinition.FullName));
            }
            var excludeAdviceAttributes = node.CustomAttributes.Where(ca => ca.AttributeType.SafeEquivalent(context.ExcludeAdviceAttributeType));
            foreach (var excludeAdviceAttribute in excludeAdviceAttributes)
            {
                var rule = new PointcutSelectorRule();
                // full names wildcards
                if (excludeAdviceAttribute.ConstructorArguments.Count == 1)
                    rule.Names.AddRange(GetStrings(excludeAdviceAttribute.ConstructorArguments[0].Value));

                // then named properties
                foreach (var namedArgument in excludeAdviceAttribute.NamedArguments)
                {
                    // names (which should usually not happen)
                    if (namedArgument.Name == nameof(ExcludeAdvicesAttribute.AdvicesTypes))
                        rule.Names.AddRange(GetStrings(namedArgument.Value));
                }
                adviceSelector.ExcludeRules.Add(rule);
            }
            if (node.Parent != null)
                adviceSelector = GetAdviceSelector(node.Parent, context) + adviceSelector;
            return adviceSelector;
        }

        /// <summary>
        /// Gets the rules at given advice.
        /// </summary>
        /// <param name="adviceType">Type of the advice.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutSelector GetPointcutSelector(TypeDef adviceType, WeavingContext context)
        {
            PointcutSelector pointcutRules;
            if (context.AdvicesRules.TryGetValue(adviceType, out pointcutRules))
                return pointcutRules;
            context.AdvicesRules[adviceType] = pointcutRules = CreatePointcutSelector(adviceType, context);
            return pointcutRules;
        }

        /// <summary>
        /// Creates the pointcut rules for a given advice.
        /// </summary>
        /// <param name="adviceTypeDef">The advice type definition.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutSelector CreatePointcutSelector(TypeDef adviceTypeDef, WeavingContext context)
        {
            var rules = new PointcutSelector();
            foreach (var customAttribute in adviceTypeDef.CustomAttributes)
                rules += CreatePointcutSelector(customAttribute, context);
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
        private PointcutSelector CreatePointcutSelector(CustomAttribute customAttribute, WeavingContext context)
        {
            var rules = new PointcutSelector();
            rules.IncludeRules.AddRange(CreatePointcutSelectorRule(customAttribute, context.IncludePointcutAttributeType));
            rules.ExcludeRules.AddRange(CreatePointcutSelectorRule(customAttribute, context.ExcludePointcutAttributeType));
            return rules;
        }

        /// <summary>
        /// Creates the pointcut rule from the given custom attribute of the the given type.
        /// </summary>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <param name="pointcutAttributeType">Type of the pointcut attribute.</param>
        /// <returns></returns>
        private IEnumerable<PointcutSelectorRule> CreatePointcutSelectorRule(CustomAttribute customAttribute, ITypeDefOrRef pointcutAttributeType)
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
