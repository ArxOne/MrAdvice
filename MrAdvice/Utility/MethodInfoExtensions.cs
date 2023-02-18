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
            return (methodAttributes & MethodAttributes.MemberAccessMask) switch
            {
                MethodAttributes.Private => VisibilityScope.PrivateMember,
                MethodAttributes.FamANDAssem => VisibilityScope.FamilyAndAssemblyMember,
                MethodAttributes.Assembly => VisibilityScope.AssemblyMember,
                MethodAttributes.Family => VisibilityScope.FamilyMember,
                MethodAttributes.FamORAssem => VisibilityScope.FamilyOrAssemblyMember,
                MethodAttributes.Public => VisibilityScope.PublicMember,
                _ => 0
            };
        }

        /// <summary>
        /// Gets a <see cref="MemberKind"/> that describes the <see cref="MethodBase"/>.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns></returns>
        public static MemberKind GetMemberKind(this MethodBase methodBase)
        {
            if (methodBase is not MethodInfo methodInfo) 
                return MemberKind.Constructor;
            if (!methodBase.IsSpecialName) 
                return MemberKind.Method;
            if (methodInfo.Name.StartsWith("get_"))
                return MemberKind.PropertyGet;
            if (methodInfo.Name.StartsWith("set_"))
                return MemberKind.PropertySet;
            if (methodInfo.Name.StartsWith("add_"))
                return MemberKind.EventAdd;
            if (methodInfo.Name.StartsWith("remove_"))
                return MemberKind.EventRemove;

            return MemberKind.Method;

        }
    }
}
