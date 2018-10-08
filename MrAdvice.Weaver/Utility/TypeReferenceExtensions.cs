#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using System.Reflection;
    using dnlib.DotNet;
    using StitcherBoy.Logging;

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
        public static bool SafeEquivalent(this ITypeDefOrRef a, ITypeDefOrRef b)
        {
            if (a is null || b is null)
                return a is null == b is null;
            return a.FullName == b.FullName;
        }

        /// <summary>
        /// Determines if two TypeReferences are equivalent.
        /// Because sadly, this feature is not implemented in TypeReference
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static bool SafeEquivalent(this TypeSig a, TypeSig b)
        {
            if (a is null || b is null)
                return a is null == b is null;
            return a.FullName == b.FullName;
        }

        /// <summary>
        /// Determines if two <see cref="IMethodDefOrRef"/> are equivalent.
        /// Because sadly, this feature is not implemented in TypeReference
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="fullCompare">if set to <c>true</c> [full compare].</param>
        /// <returns></returns>
        public static bool SafeEquivalent(this IMethod a, IMethod b, bool fullCompare = false)
        {
            if (a is null || b is null)
                return a is null == b is null;
            if (fullCompare && a.NumberOfGenericParameters != b.NumberOfGenericParameters)
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
            if (a is null || b is null)
                return a is null == b is null;
            if (fullCompare && a.GetGenericArguments().Length != b.GetGenericArguments().Length)
                return false;
            return a.DeclaringType.FullName == b.DeclaringType.FullName && a.Name == b.Name;
        }

#if NO
        /// <summary>
        /// Resolves the specified assembly resolver.
        /// </summary>
        /// <param name="typeDefOrRef">The type definition or reference.</param>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="logging">The logging.</param>
        /// <returns></returns>
        public static TypeDef ResolveTypeDef(this ITypeDefOrRef typeDefOrRef, IAssemblyResolver assemblyResolver, ILogging logging)
        {
            var typeDef = typeDefOrRef as TypeDef;
            if (typeDef != null)
                return typeDef;

            foreach (var module in typeDefOrRef.Module.GetSelfAndReferences(assemblyResolver, false, 2, logging))
            {
                typeDef = module.Find(typeDefOrRef);
                if (typeDef != null)
                    return typeDef;
            }

            return null;
        }
#endif
    }
}
