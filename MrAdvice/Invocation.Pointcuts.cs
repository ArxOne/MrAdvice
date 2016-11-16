#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Advice;
    using Annotation;
    using Aspect;
    using Pointcut;
    using Utility;

    partial class Invocation
    {
        private static readonly IDictionary<Type, PointcutSelector> PointcutSelectors = new Dictionary<Type, PointcutSelector>();

        private static bool Select(MethodBase targetMethod, AdviceInfo advice)
        {
            var reflectionName = $"{targetMethod.DeclaringType.FullName}.{targetMethod.Name}";
            var memberAttributes = targetMethod.Attributes.ToMemberAttributes()
                | targetMethod.DeclaringType.GetInformationReader().Attributes.ToMemberAttributes();
            return GetPointcutSelector(advice.Advice.GetType()).Select(reflectionName, memberAttributes);
        }

        /// <summary>
        /// Gets the exclude selector.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns></returns>
        private static PointcutSelector GetAdviceSelector(MethodBase methodBase)
        {
            var pointcutSelector = new PointcutSelector();
            // 1. some special case, avoid type from advising it self
            if (typeof(IAdvice).GetAssignmentReader().IsAssignableFrom(methodBase.DeclaringType))
                pointcutSelector.ExcludeRules.Add(new PointcutSelectorRule(methodBase.DeclaringType.FullName));
            // 2. get from method
            foreach (var methodExclude in methodBase.GetAttributes<ExcludeAdvicesAttribute>())
                pointcutSelector.ExcludeRules.Add(new PointcutSelectorRule(methodExclude.AdvicesTypes));
            // 3. from property, if any
            var propertyInfo = GetPropertyInfo(methodBase);
            if (propertyInfo != null)
                foreach (var propertyExclude in propertyInfo.Item1.GetAttributes<ExcludeAdvicesAttribute>())
                    pointcutSelector.ExcludeRules.Add(new PointcutSelectorRule(propertyExclude.AdvicesTypes));
            // 4. from type and outer types
            for (var type = methodBase.DeclaringType; type != null; type = type.DeclaringType)
                foreach (var typeExclude in type.GetInformationReader().GetAttributes<ExcludeAdvicesAttribute>())
                    pointcutSelector.ExcludeRules.Add(new PointcutSelectorRule(typeExclude.AdvicesTypes));
            // 5. from assembly
            foreach (var assemblyExclude in methodBase.DeclaringType.GetInformationReader().Assembly.GetAttributes<ExcludeAdvicesAttribute>())
                pointcutSelector.ExcludeRules.Add(new PointcutSelectorRule(assemblyExclude.AdvicesTypes));
            return pointcutSelector;
        }

        /// <summary>
        /// Gets the <see cref="PointcutSelector"/> related to given advice attribute type.
        /// </summary>
        /// <param name="adviceType">Type of the advice.</param>
        /// <returns></returns>
        private static PointcutSelector GetPointcutSelector(Type adviceType)
        {
            lock (PointcutSelectors)
            {
                PointcutSelector pointcutSelector;
                if (PointcutSelectors.TryGetValue(adviceType, out pointcutSelector))
                    return pointcutSelector;

                PointcutSelectors[adviceType] = pointcutSelector = CreatePointcutSelector(adviceType);
                return pointcutSelector;
            }
        }

        /// <summary>
        /// Creates a <see cref="PointcutSelector"/> for given advice attribute.
        /// </summary>
        /// <param name="adviceType">Type of the advice.</param>
        /// <returns></returns>
        private static PointcutSelector CreatePointcutSelector(Type adviceType)
        {
            var pointcutSelector = new PointcutSelector();
            foreach (PointcutAttribute pointcutAttribute in adviceType.GetInformationReader().GetCustomAttributes(typeof(PointcutAttribute), true))
            {
                var includeRule = CreatePointcutSelectorRule(typeof(IncludePointcutAttribute), pointcutAttribute);
                if (includeRule != null)
                    pointcutSelector.IncludeRules.Add(includeRule);
                var excludeRule = CreatePointcutSelectorRule(typeof(ExcludePointcutAttribute), pointcutAttribute);
                if (excludeRule != null)
                    pointcutSelector.ExcludeRules.Add(excludeRule);
            }
            return pointcutSelector;
        }

        /// <summary>
        /// Creates the <see cref="PointcutSelectorRule"/> from a <see cref="PointcutAttribute"/>.
        /// </summary>
        /// <param name="pointcutAttributeType">Type of the pointcut attribute.</param>
        /// <param name="pointcutAttribute">The pointcut attribute.</param>
        /// <returns></returns>
        private static PointcutSelectorRule CreatePointcutSelectorRule(Type pointcutAttributeType, PointcutAttribute pointcutAttribute)
        {
            if (!pointcutAttributeType.GetAssignmentReader().IsInstanceOfType(pointcutAttribute))
                return null;

            var rule = new PointcutSelectorRule();
            if (pointcutAttribute.Names != null)
                rule.Names.AddRange(pointcutAttribute.Names);
            rule.Attributes = pointcutAttribute.Attributes;
            return rule;
        }
    }
}
