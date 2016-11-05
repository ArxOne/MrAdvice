#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Advice;
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
        /// <returns>A list of Advices applied to method, or null if method is not advised</returns>
        public static IAdvice[] Get(MethodBase methodBase)
        {
            // method here is not supposed to change AspectInfos,
            // so just try to read it
            AspectInfo aspectInfo;
            if (Invocation.AspectInfos.TryGetValue(methodBase, out aspectInfo))
                return GetAdvices(aspectInfo.Advices);

            // then, the slow way, create it
            Tuple<PropertyInfo, bool> relatedPropertyInfo;
            var advices = Invocation.GetAdvices<IAdvice>(methodBase, out relatedPropertyInfo);
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
