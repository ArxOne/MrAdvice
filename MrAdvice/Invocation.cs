#region Weavisor
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Annotation;
    using Aspect;
    using Utility;

    /// <summary>
    /// Exposes a method to start advisors chain call
    /// This class is public, since call from generated assembly. 
    /// Semantically, it is internal.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static partial class Invocation
    {
        private static readonly IDictionary<MethodBase, Aspect.AspectInfo> AdviceChains = new Dictionary<MethodBase, Aspect.AspectInfo>();

        /// <summary>
        /// Runs a method interception.
        /// We use a static method here, if one day we want to reuse Invocations or change mecanism,
        /// it will be easier from C# code
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="methodBase">The raw method base.</param>
        /// <param name="innerMethod">The inner method.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static object ProceedAdvice(object target, object[] parameters, MethodBase methodBase, MethodInfo innerMethod)
        {
            Aspect.AspectInfo aspectInfo;
            lock (AdviceChains)
            {
                if (!AdviceChains.TryGetValue(methodBase, out aspectInfo))
                    AdviceChains[methodBase] = aspectInfo = CreateCallContext(methodBase, innerMethod);
            }

            // from here, we build an advice chain, with at least one final advice: the one who calls the method
            var adviceValues = new AdviceValues(target, parameters);
            // at least there is one context
            AdviceContext adviceContext = new InnerMethodContext(adviceValues, aspectInfo.PointcutMethod);
            foreach (var advice in aspectInfo.Advices.Reverse())
            {
                if (advice.MethodAdvice != null)
                    adviceContext = new MethodAdviceContext(advice.MethodAdvice, methodBase, adviceValues, adviceContext);
                if (advice.PropertyAdvice != null)
                    adviceContext = new PropertyAdviceContext(advice.PropertyAdvice, aspectInfo.PointcutProperty, aspectInfo.IsPointcutPropertySetter, adviceValues, adviceContext);
                if (advice.ParameterAdvice != null)
                {
                    var parameterIndex = advice.ParameterIndex.Value;
                    var parameterInfo = GetParameterInfo(methodBase, parameterIndex);
                    adviceContext = new ParameterAdviceContext(advice.ParameterAdvice, parameterInfo, parameterIndex, adviceValues, adviceContext);
                }
            }

            adviceContext.Invoke();
            return adviceValues.ReturnValue;
        }

        /// <summary>
        /// Gets the parameter information.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        /// <returns></returns>
        private static ParameterInfo GetParameterInfo(MethodBase methodBase, int parameterIndex)
        {
            if (parameterIndex >= 0)
                return methodBase.GetParameters()[parameterIndex];
            return ((MethodInfo)methodBase).ReturnParameter;
        }

        /// <summary>
        /// Processes the info advices.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        // ReSharper disable once UnusedMember.Global
        public static void ProcessInfoAdvices(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                ProcessInfoAdvices(type);
        }

        /// <summary>
        /// Processes the info advices.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void ProcessInfoAdvices(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach (var methodInfo in type.GetMethods(bindingFlags))
                ProcessMethodInfoAdvices(methodInfo);
            foreach (var constructorInfo in type.GetConstructors(bindingFlags))
                ProcessMethodInfoAdvices(constructorInfo);
            foreach (var propertyInfo in type.GetProperties(bindingFlags))
            {
                ProcessMethodInfoAdvices(propertyInfo.GetGetMethod());
                ProcessMethodInfoAdvices(propertyInfo.GetSetMethod());
                ProcessPropertyInfoAdvices(propertyInfo);
            }
        }

        /// <summary>
        /// Processes the info advices for MethodInfo.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        private static void ProcessMethodInfoAdvices(MethodBase methodInfo)
        {
            if (methodInfo == null)
                return;
            var methodInfoAdvices = GetAttributes<IMethodInfoAdvice>(methodInfo);
            foreach (var methodInfoAdvice in methodInfoAdvices)
            {
                // actually, introducing fields does not make sense here, until we introduce static fields
                SafeInjectIntroducedFields(methodInfoAdvice as IAdvice, methodInfo.DeclaringType);
                methodInfoAdvice.Advise(new MethodInfoAdviceContext(methodInfo));
            }
        }

        /// <summary>
        /// Processes the info advices for PropertyInfo.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        private static void ProcessPropertyInfoAdvices(PropertyInfo propertyInfo)
        {
            var propertyInfoAdvices = GetAttributes<IPropertyInfoAdvice>(propertyInfo);
            foreach (var propertyInfoAdvice in propertyInfoAdvices)
            {
                SafeInjectIntroducedFields(propertyInfoAdvice as IAdvice, propertyInfo.DeclaringType);
                propertyInfoAdvice.Advise(new PropertyInfoAdviceContext(propertyInfo));
            }
        }

        /// <summary>
        /// Creates the method call context, given a calling method and the inner method name.
        /// </summary>
        /// <param name="methodBase">The method information.</param>
        /// <param name="innerMethod">Name of the inner method.</param>
        /// <returns></returns>
        private static Aspect.AspectInfo CreateCallContext(MethodBase methodBase, MethodInfo innerMethod)
        {
            Tuple<PropertyInfo, bool> relatedPropertyInfo;
            var advices = GetAdvices<IAdvice>(methodBase, out relatedPropertyInfo);
            foreach (var advice in advices.Distinct())
                InjectIntroducedFields(advice.Advice, methodBase.DeclaringType);
            return new Aspect.AspectInfo
            {
                Advices = advices,
                PointcutMethod = innerMethod,
                PointcutProperty = relatedPropertyInfo != null ? relatedPropertyInfo.Item1 : null,
                IsPointcutPropertySetter = relatedPropertyInfo != null ? relatedPropertyInfo.Item2 : false
            };
        }

        /// <summary>
        /// Gets all advices available for this method.
        /// </summary>
        /// <typeparam name="TAdvice">The type of the advice.</typeparam>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="relatedPropertyInfo">The related property information.</param>
        /// <returns></returns>
        private static IList<AdviceInfo> GetAdvices<TAdvice>(MethodBase targetMethod, out Tuple<PropertyInfo, bool> relatedPropertyInfo)
            where TAdvice : class, IAdvice
        {
            var typeAndParents = targetMethod.DeclaringType.GetSelfAndParents().ToArray();
            var assemblyAndParents = typeAndParents.Select(t => t.Assembly).Distinct();

            // advices down to method
            var allAdvices = Enumerable.Union<TAdvice>(assemblyAndParents.SelectMany(GetAttributes<TAdvice>), typeAndParents.SelectMany(GetAttributes<TAdvice>))
                .Union(GetAttributes<TAdvice>(targetMethod)).Select(CreateAdvice);

            // optional from property
            relatedPropertyInfo = GetPropertyInfo(targetMethod);
            if (relatedPropertyInfo != null)
                allAdvices = allAdvices.Union(GetAttributes<TAdvice>(relatedPropertyInfo.Item1).Select(CreateAdvice));

            // and parameters (not union but concat, because same attribute may be applied at different levels)
            // ... indexed parameters
            var parameters = targetMethod.GetParameters();
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                var index = parameterIndex;
                allAdvices = allAdvices.Concat(GetAttributes<TAdvice>(parameters[parameterIndex]).Select(a => CreateAdviceIndex(a, index)));
            }
            // ... return value
            var methodInfo = targetMethod as MethodInfo;
            if (methodInfo != null)
                allAdvices = allAdvices.Concat(GetAttributes<TAdvice>(methodInfo.ReturnParameter).Select(a => CreateAdviceIndex(a, -1)));

            var advices = allAdvices.OrderByDescending(a => Priority.GetLevel(a.Advice)).ToArray();
            return advices;
        }

        private static AdviceInfo CreateAdvice<TAdvice>(TAdvice advice)
            where TAdvice : class, IAdvice
        {
            return new AdviceInfo(advice);
        }

        private static AdviceInfo CreateAdviceIndex<TAdvice>(TAdvice advice, int index)
            where TAdvice : class, IAdvice
        {
            return new AdviceInfo(advice, index);
        }

        /// <summary>
        /// Gets the advices at assembly level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(Assembly provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at type level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(Type provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at method level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(MemberInfo provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at parameter level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(ParameterInfo provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the PropertyInfo, related to a method.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns>A tuple with the PropertyInfo and true is method is a setter (false for a getter)</returns>
        private static Tuple<PropertyInfo, bool> GetPropertyInfo(MemberInfo memberInfo)
        {
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo == null || !methodInfo.IsSpecialName)
                return null;

            var isGetter = methodInfo.Name.StartsWith("get_");
            var isSetter = methodInfo.Name.StartsWith("set_");
            if (!isGetter && !isSetter)
                return null;

            // hard-coded, because the property name generates two methods: "get_xxx" and "set_xxx" where xxx is the property name
            var propertyName = methodInfo.Name.Substring(4);
            // now try to find the property
            var propertyInfo = methodInfo.DeclaringType.GetProperty(propertyName);
            if (propertyInfo == null)
                return null; // this should never happen

            return Tuple.Create(propertyInfo, isSetter);
        }
    }
}
