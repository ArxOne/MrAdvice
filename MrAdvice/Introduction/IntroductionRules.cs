#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Introduction
{
    /// <summary>
    /// Introduction rules, shared between weaver and MrAdvice assemblies
    /// </summary>
    internal static class IntroductionRules
    {
        public const string RegistryName = "<>\u03BA_IntroducedFieldsRegistry";

        /// <summary>
        /// Gets a unique name for an introduced field, related to advice and advice member name.
        /// </summary>
        /// <param name="adviceNamespace">The advice namespace.</param>
        /// <param name="adviceName">Name of the advice.</param>
        /// <param name="advisedMemberName">Name of the advised member, the target which receives advice</param>
        /// <param name="adviceMemberName">Name of the advice member (the introduced field name).</param>
        /// <returns></returns>
        public static string GetName(string adviceNamespace, string adviceName, string advisedMemberName, string adviceMemberName)
        {
            if (advisedMemberName is null) // shared advices are not related to an advised member
                return $"<{adviceNamespace}.{adviceName}.{adviceMemberName}>\u03BA__IntroducedField";
            //return $".{adviceNamespace}.{adviceName}.{adviceMemberName}";

            return $"<{adviceNamespace}.{adviceName}.{adviceMemberName}>\u03BA__IntroducedField_{advisedMemberName}";
            //return $".{adviceNamespace}.{adviceName}.{advisedMemberName}.{adviceMemberName}";
        }
    }
}
