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
    public class Invocation
    {
        private class CallContext
        {
            public IList<IMethodAdvice> Advices;
            public MethodInfo InnerMethod;
        }

        private static readonly IDictionary<MethodInfo, CallContext> CallContexts = new Dictionary<MethodInfo, CallContext>();

        /// <summary>
        /// Runs an interception.
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
            var methodInfo = methodBase as MethodInfo;
            // MethodInfo case
            if (methodInfo != null)
            {
                CallContext callContext;
                lock (CallContexts)
                {
                    if (!CallContexts.TryGetValue(methodInfo, out callContext))
                        CallContexts[methodInfo] = callContext = CreateCallContext(methodInfo, innerMethodName);
                }

                var invocation = new MethodCallContext(target, parameters, methodInfo, callContext.InnerMethod, callContext.Advices);
                invocation.Proceed(0);
                return invocation.ReturnValue;
            }

            // Ctor case
            // TODO implement ctors
            throw new NotImplementedException();
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
        /// Creates the call context, given a calling method and the inner method name.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="innerMethodName">Name of the inner method.</param>
        /// <returns></returns>
        private static CallContext CreateCallContext(MethodInfo methodInfo, string innerMethodName)
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
            var advices = GetAdvices<IMethodAdvice>(methodInfo);
            return new CallContext
            {
                Advices = advices,
                InnerMethod = innerMethod
            };
        }

        /// <summary>
        /// Gets all advices available for this method.
        /// </summary>
        /// <param name="targetMethod">The target method.</param>
        /// <returns></returns>
        private static IList<TAdvice> GetAdvices<TAdvice>(MethodInfo targetMethod)
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
