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
        public static MemberAttributes GetMemberAttributes(this MethodBase methodBase) => ToMemberAttributes(methodBase.Attributes);

        /// <summary>
        /// Converts <see cref="MethodAttributes"/> to <see cref="MemberAttributes"/>.
        /// </summary>
        /// <param name="methodAttributes">The method attributes.</param>
        /// <returns></returns>
        public static MemberAttributes ToMemberAttributes(this MethodAttributes methodAttributes)
        {
            switch (methodAttributes & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.Private: // 1
                    return MemberAttributes.PrivateMember;
                case MethodAttributes.FamANDAssem: // 2
                    return MemberAttributes.FamilyAndAssemblyMember;
                case MethodAttributes.Assembly: // 3
                    return MemberAttributes.AssemblyMember;
                case MethodAttributes.Family: // 4
                    return MemberAttributes.FamilyMember;
                case MethodAttributes.FamORAssem: // 5
                    return MemberAttributes.FamilyOrAssemblyMember;
                case MethodAttributes.Public: // 6
                    return MemberAttributes.PublicMember;
                default: // WTF?
                    return 0;
            }
        }
    }
}
