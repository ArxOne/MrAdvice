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
    using global::MrAdvice.Annotation;
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
        private IEnumerable<PointcutSelector> GetPointcutSelectors(MarkedNode markedNode, WeavingContext context)
        {
            return markedNode.Definitions.Select(d => GetPointcutSelector(d.Type, context));
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
            if (node is TypeReflectionNode typeReflectionNode && IsMarker(typeReflectionNode.TypeDefinition, context.AdviceInterfaceType))
            {
                Logging.WriteDebug("Excluding {0} from itself", typeReflectionNode.TypeDefinition.FullName);
                adviceSelector.ExcludeRules.Add(new PointcutSelectorRule(typeReflectionNode.TypeDefinition.FullName));
            }
            if (context.ExcludeAdviceAttributeType != null)
            {
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
            if (context.AdvicesRules.TryGetValue(adviceType, out var pointcutRules))
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
            var pointcutSelector = new PointcutSelector();
            foreach (var customAttribute in adviceTypeDef.CustomAttributes)
                pointcutSelector += CreatePointcutSelector(customAttribute, context);
            var baseType = adviceTypeDef.BaseType.ResolveTypeDef();
            if (baseType != null)
                pointcutSelector += GetPointcutSelector(baseType, context);
            return pointcutSelector;
        }

        /// <summary>
        /// Creates the pointcut rules from an advice attribute.
        /// The custom attribute provided has to be either <see cref="ExcludePointcutAttribute"/> or <see cref="IncludePointcutAttribute"/>
        /// If not the returned <see cref="PointcutSelector"/> is empty
        /// </summary>
        /// <param name="customAttribute">The custom attribute.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private PointcutSelector CreatePointcutSelector(CustomAttribute customAttribute, WeavingContext context)
        {
            var pointcutSelector = new PointcutSelector();
            if (context.IncludePointcutAttributeType != null)
                pointcutSelector.IncludeRules.AddRange(CreatePointcutSelectorRule(customAttribute, context.IncludePointcutAttributeType));
            if (context.ExcludePointcutAttributeType != null)
                pointcutSelector.ExcludeRules.AddRange(CreatePointcutSelectorRule(customAttribute, context.ExcludePointcutAttributeType));
            return pointcutSelector;
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
                if (namedArgument.Name == "Attributes" || namedArgument.Name == nameof(PointcutAttribute.Scope))
                    rule.Scope = (VisibilityScope)namedArgument.Value;
                // kind
                if (namedArgument.Name == nameof(PointcutAttribute.Kind))
                    rule.Kind = (MemberKind)namedArgument.Value;
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
