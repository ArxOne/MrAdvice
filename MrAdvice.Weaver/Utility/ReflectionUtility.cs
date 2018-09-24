#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using dnlib.DotNet;

    /// <summary>
    /// What is extreme laziness? :)
    /// This class allows to get PropertyInfo or MethodInfo given a lambda (which allows to refactor members)
    /// </summary>
    public static class ReflectionUtility
    {
        /// <summary>
        /// Gets the method info from lambda.
        /// </summary>
        /// <param name="lambdaExpression">The lambda expression.</param>
        /// <returns></returns>
        private static MethodInfo GetMethodInfoFromLambda(LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is MethodCallExpression methodCallExpression)
                return methodCallExpression.Method;
            var memberExpression = lambdaExpression.Body as MemberExpression;
            var propertyInfo = memberExpression?.Member as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetGetMethod();
            throw new ArgumentException("Lambda expression is not correctly formatted for MethodInfo extraction");
        }

        /// <summary>
        /// Returns a method info, providing a lambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo<T>(this Expression<Action<T>> lambdaExpression)
        {
            return GetMethodInfoFromLambda(lambdaExpression);
        }

        /// <summary>
        /// Returns a method info, providing a lambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR">The type of the R.</typeparam>
        /// <param name="lambdaExpression">The lambda expression.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo<T, TR>(this Expression<Func<T, TR>> lambdaExpression)
        {
            return GetMethodInfoFromLambda(lambdaExpression);
        }

        /// <summary>
        /// Returns a method info, for example ReflectionUtility.GetMethodInfo(() => A.F()) would return "A.F()" MethodInfo
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(this Expression<Action> lambdaExpression)
        {
            return GetMethodInfoFromLambda(lambdaExpression);
        }

        /// <summary>
        /// Extracts a MemberInfo with more or less digging
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static MemberInfo GetMemberInfoFromExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                // the ReflectionUtility Get** methods return the value as a object. If the value is a struct, we get a cast,
                // that we must unwrap
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return GetMemberInfoFromExpression(((UnaryExpression)expression).Operand);
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member;
                default:
                    throw new ArgumentException("Lambda expression is not correctly formatted for MemberInfo extraction");
            }
        }

        /// <summary>
        /// Returns MemberInfo specified in the lambda
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        private static MemberInfo GetMemberInfoFromLambda(LambdaExpression lambdaExpression)
        {
            return GetMemberInfoFromExpression(lambdaExpression.Body);
        }

        /// <summary>
        /// Returns a PropertyInfo, given a lambda
        /// For example: ReflectionUtility.GetPropertyInfo&lt;A>(a => a.Prop) would return PropertyInfo "A.Prop"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> lambdaExpression)
        {
            return (PropertyInfo)GetMemberInfoFromLambda(lambdaExpression);
        }

        /// <summary>
        /// Returns a PropertyInfo, given a lambda
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo(this Expression<Func<object>> lambdaExpression)
        {
            return (PropertyInfo)GetMemberInfoFromLambda(lambdaExpression);
        }

        /// <summary>
        /// Returns an EventInfo, given a lambda
        /// For example: ReflectionUtility.GetPropertyInfo&lt;A>(a => a.Ev) would return PropertyInfo "A.Ev"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static EventInfo GetEventInfo<T>(this Expression<Func<T, object>> lambdaExpression)
        {
            return (EventInfo)GetMemberInfoFromLambda(lambdaExpression);
        }

        /// <summary>
        /// Returns a MemberInfo, given a lambda
        /// For example: ReflectionUtility.GetMemberInfo&lt;A>(a => a.Prop) would return MemberInfo "A.Prop"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberInfo<T>(this Expression<Func<T, object>> lambdaExpression)
        {
            return GetMemberInfoFromLambda(lambdaExpression);
        }

        /// <summary>
        /// Returns a MemberInfo, given a lambda
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberInfo(this Expression<Func<object>> lambdaExpression)
        {
            return GetMemberInfoFromLambda(lambdaExpression);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value.</param>
        internal static void SetMemberValueInternal(this MemberInfo memberInfo, object instance, object value)
        {
            if (memberInfo is FieldInfo fieldInfo)
                fieldInfo.SetValue(instance, value);
            else if (memberInfo is PropertyInfo info)
                info.GetSetMethod(true).Invoke(instance, new[] { value });
            else
                throw new ArgumentException("memberInfo");
        }

        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns></returns>
        internal static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo info)
                return info.FieldType;
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;
            throw new ArgumentException("memberInfo");
        }

        /// <summary>
        /// Determines whether the given method is part of a property (getter or setter).
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        internal static bool IsPropertyMethod(this MethodDef methodDefinition)
        {
            if (!methodDefinition.IsSpecialName)
                return false;
            return methodDefinition.Name.StartsWith("get_") || methodDefinition.Name.StartsWith("set_");
        }

        /// <summary>
        /// Determines whether the given method is part of an event (add/remove).
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        internal static bool IsEventMethod(this MethodDef methodDefinition)
        {
            if (!methodDefinition.IsSpecialName)
                return false;
            return methodDefinition.Name.StartsWith("add_") || methodDefinition.Name.StartsWith("remove_");
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        internal static string GetPropertyName(string methodName)
        {
            return methodName.Substring(4);
        }
    }
}
