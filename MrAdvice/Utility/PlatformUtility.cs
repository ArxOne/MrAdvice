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
    using System.Diagnostics;
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
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the typed attributes.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="attributeProvider">The attribute provider.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Creates a delegate from a given method.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="methodInfo">The method information.</param>
        /// <returns></returns>
        public static TDelegate CreateDelegate<TDelegate>(MethodInfo methodInfo)
        {
            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), methodInfo);
        }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Assembly GetAssembly(this Type type)
        {
            return type.Assembly;
        }

        /// <summary>
        /// Gets the calling assembly.
        /// </summary>
        /// <returns></returns>
        public static Assembly GetCallingAssembly()
        {
            var st = new StackTrace();
            var thisAssembly = Assembly.GetExecutingAssembly();
            return (from frame in st.GetFrames()
                    let assembly = frame.GetMethod().DeclaringType.Assembly
                    where !assembly.Equals(thisAssembly)
                    select assembly).FirstOrDefault();
        }
    }
}
