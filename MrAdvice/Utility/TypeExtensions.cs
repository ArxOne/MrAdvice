﻿#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Annotation;

    /// <summary>
    /// Extensions to type
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Enumerates from type to topmost parent
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSelfAndAncestors(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.GetInformationReader().BaseType;
            }
        }

        /// <summary>
        /// Enumerates from type to topmost parent
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        [Obsolete("Use GetSelfAndAncestors() instead")]
        public static IEnumerable<Type> GetSelfAndParents(this Type type) => GetSelfAndAncestors(type);

        /// <summary>
        /// Gets the self and enclosing types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSelfAndEnclosing(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.DeclaringType;
            }
        }

        /// <summary>
        /// Gets the fields and properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type, BindingFlags bindingFlags)
        {
            return type.GetMembersReader().GetFields(bindingFlags).Cast<MemberInfo>()
                .Concat(type.GetMembersReader().GetProperties(bindingFlags));
        }

        /// <summary>
        /// Gets the member attributes for the given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static VisibilityScope GetVisibilityScope(this Type type)
        {
            return type.GetInformationReader().Attributes.ToVisibilityScope();
        }

        /// <summary>
        /// Converts the type attributes to member attributes.
        /// </summary>
        /// <param name="typeAttributes">The type attributes.</param>
        /// <returns></returns>
        public static VisibilityScope ToVisibilityScope(this TypeAttributes typeAttributes)
        {
            return (typeAttributes & TypeAttributes.VisibilityMask) switch
            {
                TypeAttributes.NotPublic => VisibilityScope.PrivateGlobalType,
                TypeAttributes.Public => VisibilityScope.PublicGlobalType,
                TypeAttributes.NestedPublic => VisibilityScope.PublicNestedType,
                TypeAttributes.NestedPrivate => VisibilityScope.PrivateNestedType,
                TypeAttributes.NestedFamily => VisibilityScope.FamilyNestedType,
                TypeAttributes.NestedAssembly => VisibilityScope.AssemblyNestedType,
                TypeAttributes.NestedFamANDAssem => VisibilityScope.FamilyAndAssemblyType,
                TypeAttributes.NestedFamORAssem => VisibilityScope.FamilyOrAssemblyType,
                _ => throw new ArgumentOutOfRangeException(nameof(typeAttributes))
            };
        }
    }
}
