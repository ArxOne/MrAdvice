#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System.Reflection;
    using Annotation;
    using global::MrAdvice.Annotation;

    /// <summary>
    /// Extensions for <see cref="MethodBase"/>
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Gets the member attributes for the method.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns></returns>
        public static VisibilityScope GetVisibilityScope(this MethodBase methodBase) => ToVisibilityScope(methodBase.Attributes);

        /// <summary>
        /// Converts <see cref="MethodAttributes"/> to <see cref="VisibilityScope"/>.
        /// </summary>
        /// <param name="methodAttributes">The method attributes.</param>
        /// <returns></returns>
        public static VisibilityScope ToVisibilityScope(this MethodAttributes methodAttributes)
        {
            switch (methodAttributes & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.Private: // 1
                    return VisibilityScope.PrivateMember;
                case MethodAttributes.FamANDAssem: // 2
                    return VisibilityScope.FamilyAndAssemblyMember;
                case MethodAttributes.Assembly: // 3
                    return VisibilityScope.AssemblyMember;
                case MethodAttributes.Family: // 4
                    return VisibilityScope.FamilyMember;
                case MethodAttributes.FamORAssem: // 5
                    return VisibilityScope.FamilyOrAssemblyMember;
                case MethodAttributes.Public: // 6
                    return VisibilityScope.PublicMember;
                default: // WTF?
                    return 0;
            }
        }

        /// <summary>
        /// Gets a <see cref="MemberKind"/> that describes the <see cref="MethodBase"/>.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns></returns>
        public static MemberKind GetMemberKind(this MethodBase methodBase)
        {
            if (methodBase is MethodInfo methodInfo)
            {
                if (methodBase.IsSpecialName)
                {
                    if (methodInfo.Name.StartsWith("get_"))
                        return MemberKind.PropertyGet;
                    if (methodInfo.Name.StartsWith("set_"))
                        return MemberKind.PropertySet;
                    if (methodInfo.Name.StartsWith("add_"))
                        return MemberKind.EventAdd;
                    if (methodInfo.Name.StartsWith("remove_"))
                        return MemberKind.EventRemove;
                }

                return MemberKind.Method;
            }

            return MemberKind.Constructor;
        }
    }
}
