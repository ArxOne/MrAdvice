#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using dnlib.DotNet;
    using Pointcut;
    using Utility;
    using MethodAttributes = System.Reflection.MethodAttributes;
    using TypeAttributes = System.Reflection.TypeAttributes;

    /// <summary>
    /// Extensions to <see cref="PointcutSelector"/>, to ease use from weaver
    /// </summary>
    public static class PointcutSelectorExtensions
    {
        /// <summary>
        /// Indicates whether the specified node has to be selected for advice
        /// </summary>
        /// <param name="pointcutSelector">The pointcut selector.</param>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static bool Select(this PointcutSelector pointcutSelector, MethodDef method)
        {
            var name = $"{method.DeclaringType.FullName}.{method.Name}".Replace('/', '+');
            var visibilityScope = ((MethodAttributes)method.Attributes).ToVisibilityScope() | ((TypeAttributes)method.DeclaringType.Attributes).ToVisibilityScope();
            var memberKind = method.GetMemberKind();
            return pointcutSelector.Select(name, visibilityScope, memberKind);
        }

        public static bool Select(this PointcutSelector pointcutSelector, ITypeDefOrRef type)
        {
            var name = type.FullName.Replace('/', '+');
            return pointcutSelector.Select(name, null, null);
        }
    }
}
