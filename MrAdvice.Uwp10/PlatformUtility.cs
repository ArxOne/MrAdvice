#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Utility for platform
    /// </summary>
    public static class PlatformUtility
    {
        /// <summary>
        /// Gets the members reader, which allows to read members (fields, properties, methods) from type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetMembersReader(this Type type)
        {
            return type;
        }

        /// <summary>
        /// Gets the assignment reader (to see if type can be assigned).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetAssignmentReader(this Type type)
        {
            return type;
        }

        /// <summary>
        /// Gets the information reader (allows to query base, assembly).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static TypeInfo GetInformationReader(this Type type)
        {
            return type.GetTypeInfo();
        }

        /// <summary>
        /// Gets the advices at assembly level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Assembly provider)
        {
            return provider.GetCustomAttributes().OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at type level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this TypeInfo provider)
        {
            return provider.GetCustomAttributes().OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at type level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type provider)
        {
            return provider.GetTypeInfo().GetCustomAttributes().OfType<TAttribute>();
        }

#if no
        /// <summary>
        /// Gets the advices at method level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }
#endif

        /// <summary>
        /// Gets the advices at method level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MethodBase provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at method level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this FieldInfo provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at method level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this PropertyInfo provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at parameter level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ParameterInfo provider)
        {
            // UWP may return null here
            return (provider.GetCustomAttributes(false) ?? new Attribute[0]).OfType<TAttribute>();
        }
    }
}
