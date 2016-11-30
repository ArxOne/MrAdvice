#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions to abstract <see cref="MemberInfo"/>
    /// </summary>
    internal static class MemberInfoExtensions
    {
        private static readonly object[] NoParameter = new object[0];

        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.FieldType;
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.PropertyType;
            throw new NotSupportedException(string.Format("Type {0} not supported", memberInfo.GetType()));
        }

        /// <summary>
        /// Gets the value from target using memberInfo.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static object GetValue(this MemberInfo memberInfo, object target)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.GetValue(target);
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetValue(target, NoParameter);
            throw new NotSupportedException(string.Format("Type {0} not supported", memberInfo.GetType()));
        }

        /// <summary>
        /// Sets the value to target using memberInfo.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public static void SetValue(this MemberInfo memberInfo, object target, object value)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(target, value);
                return;
            }
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(target, value, NoParameter);
                return;
            }
            throw new NotSupportedException(string.Format("Type {0} not supported", memberInfo.GetType()));
        }

        /// <summary>
        /// Determines whether the specified <see cref="MemberInfo"/> is static.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static bool IsStatic(this MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.IsStatic;
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetGetMethod().IsStatic;
            throw new NotSupportedException(string.Format("Type {0} not supported", memberInfo.GetType()));
        }

#if !WINDOWS_UWP
        /// <summary>
        /// Gets a signel custom attribute (or null).
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="memberInfo">The member information.</param>
        /// <returns></returns>
        public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo memberInfo)
            where TAttribute : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>().SingleOrDefault();
        }
#endif
    }
}
