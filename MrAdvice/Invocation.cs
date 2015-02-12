#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Annotation;
    using Introduction;
    using Utility;

    /// <summary>
    /// Exposes a method to start advisors chain call
    /// This class is public, since call from generated assembly. 
    /// Semantically, it is internal.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static class Invocation
    {
        private class AdviceChain
        {
            public IList<IAdvice> Advices;
            public MethodInfo InnerMethod;
            public PropertyInfo PropertyInfo { get; set; }
            public bool IsSetter { get; set; }
        }

        private static readonly IDictionary<MethodBase, AdviceChain> AdviceChains = new Dictionary<MethodBase, AdviceChain>();

        /// <summary>
        /// Runs a method interception.
        /// We use a static method here, if one day we want to reuse Invocations or change mecanism,
        /// it will be easier from C# code
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="methodBase">The raw method base.</param>
        /// <param name="innerMethod">The inner method.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static object ProceedAdvice(object target, object[] parameters, MethodBase methodBase, MethodInfo innerMethod)
        {
            AdviceChain adviceChain;
            lock (AdviceChains)
            {
                if (!AdviceChains.TryGetValue(methodBase, out adviceChain))
                    AdviceChains[methodBase] = adviceChain = CreateCallContext(methodBase, innerMethod);
            }

            // from here, we build an advice chain, with at least one final advice: the one who calls the method
            var adviceValues = new AdviceValues(target, parameters);
            // at least there is one context
            AdviceContext adviceContext = new InnerMethodContext(adviceValues, adviceChain.InnerMethod);
            foreach (var advice in adviceChain.Advices.Reverse())
            {
                var methodAdvice = advice as IMethodAdvice;
                if (methodAdvice != null)
                    adviceContext = new MethodAdviceContext(methodAdvice, methodBase, adviceValues, adviceContext);
                var propertyAdvice = advice as IPropertyAdvice;
                if (propertyAdvice != null)
                    adviceContext = new PropertyAdviceContext(propertyAdvice, adviceChain.PropertyInfo, adviceChain.IsSetter, adviceValues, adviceContext);
            }

            adviceContext.Invoke();
            return adviceValues.ReturnValue;
        }

        /// <summary>
        /// Processes the info advices.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        // ReSharper disable once UnusedMember.Global
        public static void ProcessInfoAdvices(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                ProcessInfoAdvices(type);
        }

        /// <summary>
        /// Processes the info advices.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void ProcessInfoAdvices(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach (var methodInfo in type.GetMethods(bindingFlags))
                ProcessMethodInfoAdvices(methodInfo);
            foreach (var constructorInfo in type.GetConstructors(bindingFlags))
                ProcessMethodInfoAdvices(constructorInfo);
            foreach (var propertyInfo in type.GetProperties(bindingFlags))
            {
                ProcessMethodInfoAdvices(propertyInfo.GetGetMethod());
                ProcessMethodInfoAdvices(propertyInfo.GetSetMethod());
                ProcessPropertyInfoAdvices(propertyInfo);
            }
        }

        /// <summary>
        /// Processes the info advices for MethodInfo.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        private static void ProcessMethodInfoAdvices(MethodBase methodInfo)
        {
            if (methodInfo == null)
                return;
            var methodInfoAdvices = GetAttributes<IMethodInfoAdvice>(methodInfo);
            foreach (var methodInfoAdvice in methodInfoAdvices)
            {
                // actually, introducing fields does not make sense here, until we introduce static fields
                SafeInjectIntroducedFields(methodInfoAdvice as IAdvice, methodInfo.DeclaringType);
                methodInfoAdvice.Advise(new MethodInfoAdviceContext(methodInfo));
            }
        }

        /// <summary>
        /// Processes the info advices for PropertyInfo.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        private static void ProcessPropertyInfoAdvices(PropertyInfo propertyInfo)
        {
            var propertyInfoAdvices = GetAttributes<IPropertyInfoAdvice>(propertyInfo);
            foreach (var propertyInfoAdvice in propertyInfoAdvices)
            {
                SafeInjectIntroducedFields(propertyInfoAdvice as IAdvice, propertyInfo.DeclaringType);
                propertyInfoAdvice.Advise(new PropertyInfoAdviceContext(propertyInfo));
            }
        }

        /// <summary>
        /// Creates the method call context, given a calling method and the inner method name.
        /// </summary>
        /// <param name="methodBase">The method information.</param>
        /// <param name="innerMethod">Name of the inner method.</param>
        /// <returns></returns>
        private static AdviceChain CreateCallContext(MethodBase methodBase, MethodInfo innerMethod)
        {
            Tuple<PropertyInfo, bool> relatedPropertyInfo;
            var advices = GetAdvices<IAdvice>(methodBase, out relatedPropertyInfo);
            foreach (var advice in advices)
                InjectIntroducedFields(advice, methodBase.DeclaringType);
            return new AdviceChain
            {
                Advices = advices,
                InnerMethod = innerMethod,
                PropertyInfo = relatedPropertyInfo != null ? relatedPropertyInfo.Item1 : null,
                IsSetter = relatedPropertyInfo != null ? relatedPropertyInfo.Item2 : false
            };
        }

        private static void SafeInjectIntroducedFields(IAdvice advice, Type advisedType)
        {
            // shame, but easy here
            if (advice == null)
                return;
            InjectIntroducedFields(advice, advisedType);
        }

        /// <summary>
        /// Injects the introduced fields to advice.
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="advisedType">Type of the advised.</param>
        private static void InjectIntroducedFields(IAdvice advice, Type advisedType)
        {
            // shame, but easy here
            if (advice == null)
                return;
            const BindingFlags adviceMembersBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            const BindingFlags introducedFieldsBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            Type introducedFieldType = null;
            foreach (var memberInfo in advice.GetType().GetFieldsAndProperties(adviceMembersBindingFlags)
                .Where(f => IsIntroduction(f.GetMemberType(), out introducedFieldType)))
            {
                var memberValue = memberInfo.GetValue(advice);
                if (memberValue == null)
                    InjectIntroducedField(advice, memberInfo, advisedType, introducedFieldType, introducedFieldsBindingFlags);
            }
        }

        /// <summary>
        /// Injects the introduced field.
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="adviceMemberInfo">The member information.</param>
        /// <param name="advisedType">Type of the advised.</param>
        /// <param name="introducedFieldType">Type of the introduced field.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        private static void InjectIntroducedField(IAdvice advice, MemberInfo adviceMemberInfo, Type advisedType, Type introducedFieldType,
            BindingFlags bindingFlags)
        {
            var adviceType = advice.GetType();
            var introducedFieldName = IntroductionRules.GetName(adviceType.Namespace, adviceType.Name, adviceMemberInfo.Name);
            var linkID = string.Format("{0}:{1}", adviceType.AssemblyQualifiedName, adviceMemberInfo.Name);
            var introducedField = FindIntroducedFieldByName(advisedType, introducedFieldName, linkID, bindingFlags)
                ?? FindIntroducedFieldByTypeAndAvailability(advisedType, introducedFieldType, adviceMemberInfo.IsStatic(), bindingFlags, linkID);
            if (introducedField == null)
                throw new InvalidOperationException("Internal error, can not find matching introduced field");
            var introducedFieldAttribute = introducedField.GetCustomAttribute<IntroducedFieldAttribute>();
            introducedFieldAttribute.LinkID = linkID;
            adviceMemberInfo.SetValue(advice, Activator.CreateInstance(adviceMemberInfo.GetMemberType(), introducedField));
        }

        /// <summary>
        /// Finds the introduced field in the advised class, by name.
        /// </summary>
        /// <param name="advisedType">Type of the advised.</param>
        /// <param name="introducedFieldName">Name of the introduced field.</param>
        /// <param name="linkID">The link identifier.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <returns></returns>
        private static FieldInfo FindIntroducedFieldByName(Type advisedType, string introducedFieldName, string linkID, BindingFlags bindingFlags)
        {
            var introducedField = advisedType.GetField(introducedFieldName, bindingFlags);
            if (introducedField == null)
                return null;
            var introducedFieldAttribute = introducedField.GetCustomAttribute<IntroducedFieldAttribute>();
            if (introducedFieldAttribute.LinkID != null && introducedFieldAttribute.LinkID != linkID)
                return null;
            introducedFieldAttribute.LinkID = linkID;
            return introducedField;
        }

        /// <summary>
        /// Finds the introduced field by type and availability.
        /// </summary>
        /// <param name="advisedType">Type of the advised.</param>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="isStatic">if set to <c>true</c> [is static].</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="linkID">The link identifier.</param>
        /// <returns></returns>
        private static FieldInfo FindIntroducedFieldByTypeAndAvailability(Type advisedType, Type fieldType, bool isStatic, BindingFlags bindingFlags, string linkID)
        {
            return (from fieldInfo in advisedType.GetFields(bindingFlags)
                    where fieldInfo.FieldType == fieldType
                          && fieldInfo.IsStatic == isStatic
                    let introducedFieldAttribute = fieldInfo.GetCustomAttribute<IntroducedFieldAttribute>()
                    where introducedFieldAttribute != null
                          && introducedFieldAttribute.LinkID == null
                    select fieldInfo).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the specified member type is introduction.
        /// </summary>
        /// <param name="memberType">Type of the member.</param>
        /// <param name="introducedType">Type of the introduced.</param>
        /// <returns></returns>
        private static bool IsIntroduction(Type memberType, out Type introducedType)
        {
            if (!memberType.IsGenericType || memberType.GetGenericTypeDefinition() != typeof(IntroducedField<>))
            {
                introducedType = null;
                return false;
            }
            introducedType = memberType.GetGenericArguments()[0];
            return true;
        }

        /// <summary>
        /// Determines whether the specified member type is introduction.
        /// </summary>
        /// <param name="memberType">Type of the member.</param>
        /// <returns></returns>
        private static bool IsIntroduction(Type memberType)
        {
            Type introducedType;
            return IsIntroduction(memberType, out introducedType);
        }

        /// <summary>
        /// Gets all advices available for this method.
        /// </summary>
        /// <typeparam name="TAdvice">The type of the advice.</typeparam>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="relatedPropertyInfo">The related property information.</param>
        /// <returns></returns>
        private static IList<TAdvice> GetAdvices<TAdvice>(MemberInfo targetMethod, out Tuple<PropertyInfo, bool> relatedPropertyInfo)
            where TAdvice : class, IAdvice
        {
            var typeAndParents = targetMethod.DeclaringType.GetSelfAndParents().ToArray();
            var assemblyAndParents = typeAndParents.Select(t => t.Assembly).Distinct();
            var allAdvices = Enumerable.Union(assemblyAndParents.SelectMany(GetAttributes<TAdvice>), typeAndParents.SelectMany(GetAttributes<TAdvice>))
                .Union(GetAttributes<TAdvice>(targetMethod));
            relatedPropertyInfo = GetPropertyInfo(targetMethod);
            if (relatedPropertyInfo != null)
                allAdvices = allAdvices.Union(GetAttributes<TAdvice>(relatedPropertyInfo.Item1));
            var advices = allAdvices.Distinct()
                .OrderByDescending(Priority.GetLevel).ToArray();
            return advices;
        }

        /// <summary>
        /// Gets the advices at assembly level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(Assembly provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at type level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(Type provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the advices at method level.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(MemberInfo provider)
        {
            return provider.GetCustomAttributes(false).OfType<TAttribute>();
        }

        /// <summary>
        /// Gets the PropertyInfo, related to a method.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns>A tuple with the PropertyInfo and true is method is a setter (false for a getter)</returns>
        private static Tuple<PropertyInfo, bool> GetPropertyInfo(MemberInfo memberInfo)
        {
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo == null || !methodInfo.IsSpecialName)
                return null;

            var isGetter = methodInfo.Name.StartsWith("get_");
            var isSetter = methodInfo.Name.StartsWith("set_");
            if (!isGetter && !isSetter)
                return null;

            // hard-coded, because the property name generates two methods: "get_xxx" and "set_xxx" where xxx is the property name
            var propertyName = methodInfo.Name.Substring(4);
            // now try to find the property
            var propertyInfo = methodInfo.DeclaringType.GetProperty(propertyName);
            if (propertyInfo == null)
                return null; // this should never happen

            return Tuple.Create(propertyInfo, isSetter);
        }
    }
}
