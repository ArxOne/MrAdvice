#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Introduction
{
    /// <summary>
    /// Introduction rules, shared between weaver and weavisor assemblies
    /// </summary>
    internal static class IntroductionRules
    {
        /// <summary>
        /// Gets a unique name for an introduced field, related to advice and advice member name.
        /// </summary>
        /// <param name="adviceNamespace">The advice namespace.</param>
        /// <param name="adviceName">Name of the advice.</param>
        /// <param name="adviceMemberName">Name of the advice member.</param>
        /// <returns></returns>
        public static string GetName(string adviceNamespace, string adviceName, string adviceMemberName)
        {
            return string.Format(".{0}.{1}.{2}", adviceNamespace, adviceName, adviceMemberName);
        }
    }
}
