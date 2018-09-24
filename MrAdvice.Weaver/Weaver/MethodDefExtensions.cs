#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System.Reflection;
    using dnlib.DotNet;
    using global::MrAdvice.Annotation;

    public static class MethodDefExtensions
    {
        /// <summary>
        /// Gets a <see cref="MemberKind"/> that describes the <see cref="MethodBase"/>.
        /// </summary>
        /// <param name="methodDef">The method base.</param>
        /// <returns></returns>
        public static MemberKind GetMemberKind(this MethodDef methodDef)
        {
            if (methodDef.IsConstructor)
                return MemberKind.Constructor;

            if (methodDef.IsSpecialName)
            {
                if (methodDef.Name.StartsWith("get_"))
                    return MemberKind.PropertyGet;
                if (methodDef.Name.StartsWith("set_"))
                    return MemberKind.PropertySet;
                if (methodDef.Name.StartsWith("add_"))
                    return MemberKind.EventAdd;
                if (methodDef.Name.StartsWith("remove_"))
                    return MemberKind.EventRemove;
            }

            return MemberKind.Method;
        }
    }
}
