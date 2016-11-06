#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System.Reflection;
    using Pointcut;
    using Reflection.Groups;
    using Utility;

    /// <summary>
    /// Extensions to <see cref="PointcutSelector"/>, to ease use from weaver
    /// </summary>
    public static class PointcutSelectorExtensions
    {
        /// <summary>
        /// Indicates whether the specified node has to be selected for advice
        /// </summary>
        /// <param name="pointcutSelector">The pointcut selector.</param>
        /// <param name="reflectionNode">The reflection node.</param>
        /// <returns></returns>
        public static bool Select(this PointcutSelector pointcutSelector, ReflectionNode reflectionNode)
        {
            var method = reflectionNode.Method;
            var name = $"{method.DeclaringType.FullName}.{method.Name}";
            return pointcutSelector.Select(name, ((MethodAttributes)method.Attributes).ToMemberAttributes() | ((TypeAttributes)method.DeclaringType.Attributes).ToMemberAttributes());
        }
    }
}
