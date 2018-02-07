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
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Annotation;
    using Aspect;

    /// <summary>
    /// Advices helper. The reflection for advices
    /// </summary>
    public static class Advices
    {
        /// <summary>
        /// Gets the advices applied to given method.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns>Either:
        /// - a list of Advices applied to method,
        /// - an empty array from within a weaved method body (because the tools to tell are not here yet),
        /// - null if method is not advised</returns>
        public static IAdvice[] Get(MethodBase methodBase)
        {
            if (methodBase.GetCustomAttributes(false).OfType<ExecutionPointAttribute>().Any())
                return new IAdvice[0];

            // method here is not supposed to change AspectInfos,
            // so just try to read it
            var aspectInfo = Invocation.AspectInfos.Values.SelectMany(v => v.Values).FirstOrDefault(a => Equals(a.PointcutMethod, methodBase));
            if (aspectInfo != null)
                return GetAdvices(aspectInfo.Advices);

            // then, the slow way, create it
            var advices = Invocation.GetAdvices<IAdvice>(methodBase, out _, out _);
            return GetAdvices(advices);
        }

        private static IAdvice[] GetAdvices(IEnumerable<AdviceInfo> adviceInfos)
        {
            var advices = adviceInfos.Select(a => a.Advice).ToArray();
            if (advices.Length == 0)
                return null;
            return advices;
        }
    }
}
