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
        public static Type GetInformationReader(this Type type)
        {
            return type;
        }

        /// <summary>
        /// Gets the advices at type level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type provider)
        {
            return provider.GetCustomAttributes(typeof(TAttribute), false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the typed attributes.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="attributeProvider">The attribute provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
        }
    }
}
