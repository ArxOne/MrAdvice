#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Utility;

    /// <summary>
    /// Exposes a method to start advisors chain call
    /// This class is public, since call from generated assembly. 
    /// Semantically, it is internal.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class Invocation
    {
        private static readonly IDictionary<MethodInfo, IList<IMethodAdvice>> MethodAdvices = new Dictionary<MethodInfo, IList<IMethodAdvice>>();

        /// <summary>
        /// Runs an interception.
        /// We use a static method here, if one day we want to reuse Invocations or change mecanism,
        /// it will be easier from C# code
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="targetMethod">The target method (the one which was originally called and is being advised).</param>
        /// <param name="innerMethod">The inner method (to be called at last).</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        // ReSharper disable once UnusedMember.Global
        public static object ProceedMethod(object target, object[] parameters, MethodInfo targetMethod, MethodInfo innerMethod)
        {
            IList<IMethodAdvice> methodAdvices;
            lock (MethodAdvices)
            {
                if (!MethodAdvices.TryGetValue(targetMethod, out methodAdvices))
                    MethodAdvices[targetMethod] = methodAdvices = GetAllAdvices(targetMethod);
            }

            var invocation = new MethodCallContext(target, parameters, targetMethod, innerMethod, methodAdvices);
            invocation.Proceed(0);
            return invocation.ReturnValue;
        }

        private static IList<IMethodAdvice> GetAllAdvices(MethodInfo targetMethod)
        {
            var typeAndParents = targetMethod.DeclaringType.GetSelfAndParents().ToArray();
            var assemblyAndParents = typeAndParents.Select(t => t.Assembly).Distinct();
            return assemblyAndParents.SelectMany(GetAdvices)
                .Union(typeAndParents.SelectMany(GetAdvices))
                .Union(GetAdvices(targetMethod)).Distinct().ToArray();
        }

        private static IEnumerable<IMethodAdvice> GetAdvices(Assembly provider)
        {
            return provider.GetCustomAttributes(false).OfType<IMethodAdvice>();
        }

        private static IEnumerable<IMethodAdvice> GetAdvices(MemberInfo provider)
        {
            return provider.GetCustomAttributes(false).OfType<IMethodAdvice>();
        }

        private static IEnumerable<IMethodAdvice> GetAdvices(MethodInfo provider)
        {
            return provider.GetCustomAttributes(false).OfType<IMethodAdvice>();
        }
    }
}
