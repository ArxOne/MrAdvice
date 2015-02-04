#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Annotation;
    using Utility;

    /// <summary>
    /// Exposes a method to start advisors chain call
    /// This class is public, since call from generated assembly. 
    /// Semantically, it is internal.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static class Invocation
    {
        private class MethodCallChain
        {
            public IList<IMethodAdvice> Advices;
            public MethodInfo InnerMethod;
        }

        private class ConstructorCallChain
        {
            public IList<IConstructorAdvice> Advices;
            public MethodInfo InnerMethod;
        }

        private static readonly IDictionary<MethodInfo, MethodCallChain> MethodCallContexts = new Dictionary<MethodInfo, MethodCallChain>();
        private static readonly IDictionary<ConstructorInfo, ConstructorCallChain> ConstructorCallContexts = new Dictionary<ConstructorInfo, ConstructorCallChain>();

        /// <summary>
        /// Runs a method interception.
        /// We use a static method here, if one day we want to reuse Invocations or change mecanism,
        /// it will be easier from C# code
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="methodBase">The raw method base.</param>
        /// <param name="innerMethodName">Name of the inner method.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static object ProceedMethod(object target, object[] parameters, MethodBase methodBase, string innerMethodName)
        {
            var methodInfo = (MethodInfo)methodBase;
            MethodCallChain methodCallChain;
            lock (MethodCallContexts)
            {
                if (!MethodCallContexts.TryGetValue(methodInfo, out methodCallChain))
                    MethodCallContexts[methodInfo] = methodCallChain = CreateCallContext(methodInfo, innerMethodName);
            }

            var methodInvocation = new MethodCallContext(target, parameters, methodInfo, methodCallChain.InnerMethod, methodCallChain.Advices);
            methodInvocation.Proceed(0);
            return methodInvocation.ReturnValue;
        }

        /// <summary>
        /// Runs a constructor interception.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="methodBase">The method base.</param>
        /// <param name="innerMethodName">Name of the inner method.</param>
        public static void ProceedConstructor(object target, object[] parameters, MethodBase methodBase, string innerMethodName)
        {
            var constructorInfo = (ConstructorInfo)methodBase;
            ConstructorCallChain constructorCallChain;
            lock (ConstructorCallContexts)
            {
                if (!ConstructorCallContexts.TryGetValue(constructorInfo, out constructorCallChain))
                    ConstructorCallContexts[constructorInfo] = constructorCallChain = CreateCallContext(constructorInfo, innerMethodName);
            }

            var invocation = new ConstructorCallContext(target, parameters, constructorInfo, constructorCallChain.InnerMethod, constructorCallChain.Advices);
            invocation.Proceed(0);
        }

        /// <summary>
        /// Processes the runtime initializers.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        // ReSharper disable once UnusedMember.Global
        public static void ProcessInitializers(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                foreach (var methodInfo in type.GetMethods())
                {
                    var runtimeInitializers = GetAttributes<IMethodInitializer>(methodInfo).ToArray();
                    foreach (var runtimeInitializer in runtimeInitializers)
                        runtimeInitializer.Initialize(methodInfo);
                }
        }

        /// <summary>
        /// Creates the method call context, given a calling method and the inner method name.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="innerMethodName">Name of the inner method.</param>
        /// <returns></returns>
        private static MethodCallChain CreateCallContext(MethodInfo methodInfo, string innerMethodName)
        {
            return new MethodCallChain
            {
                Advices = GetAdvices<IMethodAdvice>(methodInfo),
                InnerMethod = GetInnerMethod(methodInfo, innerMethodName)
            };
        }

        /// <summary>
        /// Creates the ctor call context, given a calling method and the inner method name.
        /// </summary>
        /// <param name="constructorInfo">The method information.</param>
        /// <param name="innerMethodName">Name of the inner method.</param>
        /// <returns></returns>
        private static ConstructorCallChain CreateCallContext(ConstructorInfo constructorInfo, string innerMethodName)
        {
            return new ConstructorCallChain
            {
                Advices = GetAdvices<IConstructorAdvice>(constructorInfo),
                InnerMethod = GetInnerMethod(constructorInfo, innerMethodName)
            };
        }

        private static MethodInfo GetInnerMethod(MethodBase methodInfo, string innerMethodName)
        {
            MethodInfo innerMethod;
            var innerMethods = methodInfo.DeclaringType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.Name == innerMethodName).ToArray();
            switch (innerMethods.Length)
            {
                case 0:
                    throw new InvalidOperationException("WTF?");
                case 1:
                    innerMethod = innerMethods[0];
                    break;
                default:
                    var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                    innerMethod = innerMethods.Single(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
                    break;
            }
            return innerMethod;
        }

        /// <summary>
        /// Gets all advices available for this method.
        /// </summary>
        /// <param name="targetMethod">The target method.</param>
        /// <returns></returns>
        private static IList<TAdvice> GetAdvices<TAdvice>(MemberInfo targetMethod)
            where TAdvice : class, IAdvice
        {
            var typeAndParents = targetMethod.DeclaringType.GetSelfAndParents().ToArray();
            var assemblyAndParents = typeAndParents.Select(t => t.Assembly).Distinct();
            var advices = assemblyAndParents.SelectMany(GetAttributes<TAdvice>)
                .Union(typeAndParents.SelectMany(GetAttributes<TAdvice>))
                .Union(GetAttributes<TAdvice>(targetMethod)).Distinct()
                .OrderByDescending(Priority.Get).ToArray();
            return advices;
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
    }
}
