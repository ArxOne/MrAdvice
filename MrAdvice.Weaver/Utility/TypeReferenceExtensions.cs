#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using System.Reflection;
    using Mono.Cecil;

    /// <summary>
    /// Extensions to TypeReference
    /// </summary>
    public static class TypeReferenceExtensions
    {
        /// <summary>
        /// Determines if two TypeReferences are equivalent.
        /// Because sadly, this feature is not implemented in TypeReference
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static bool SafeEquivalent(this TypeReference a, TypeReference b)
        {
            if (a == null || b == null)
                return (a == null) == (b == null);
            return a.FullName == b.FullName;
        }

        /// <summary>
        /// Determines if two <see cref="MethodReference"/> are equivalent.
        /// Because sadly, this feature is not implemented in TypeReference
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="fullCompare">if set to <c>true</c> [full compare].</param>
        /// <returns></returns>
        public static bool SafeEquivalent(this MethodReference a, MethodReference b, bool fullCompare = false)
        {
            if (a == null || b == null)
                return (a == null) == (b == null);
            if (fullCompare && a.GenericParameters.Count != b.GenericParameters.Count)
                return false;
            return a.DeclaringType.FullName == b.DeclaringType.FullName && a.Name == b.Name;
        }

        /// <summary>
        /// Determines if two <see cref="MethodInfo"/> are equivalent.
        /// Because sadly, this feature is not implemented in TypeReference
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="fullCompare">if set to <c>true</c> [full compare].</param>
        /// <returns></returns>
        public static bool SafeEquivalent(this MethodInfo a, MethodInfo b, bool fullCompare = false)
        {
            if (a == null || b == null)
                return (a == null) == (b == null);
            if (fullCompare && a.GetGenericArguments().Length != b.GetGenericArguments().Length)
                return false;
            return a.DeclaringType.FullName == b.DeclaringType.FullName && a.Name == b.Name;
        }
    }
}
