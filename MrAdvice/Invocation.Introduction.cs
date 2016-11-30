#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Introduction;
    using Utility;

    partial class Invocation
    {
        /// <summary>
        /// Injects the introduced fields to advice.
        /// Allows null advices (and does nothing)
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="advisedType">Type of the advised.</param>
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
            foreach (var memberInfo in advice.GetType().GetFieldsAndProperties(adviceMembersBindingFlags).Where(IsIntroduction))
            {
                var memberValue = memberInfo.GetValue(advice);
                if (memberValue == null)
                    InjectIntroducedField(advice, memberInfo, advisedType);
            }
        }

        /// <summary>
        /// Injects the introduced field.
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="adviceMemberInfo">The member information.</param>
        /// <param name="advisedType">Type of the advised.</param>
        /// <exception cref="System.InvalidOperationException">Internal error, can not find matching introduced field</exception>
        private static void InjectIntroducedField(IAdvice advice, MemberInfo adviceMemberInfo, Type advisedType)
        {
            adviceMemberInfo.SetValue(advice, Activator.CreateInstance(adviceMemberInfo.GetMemberType(), advice, adviceMemberInfo));
        }

        /// <summary>
        /// Finds the introduced field.
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="adviceMemberInfo">The advice member information.</param>
        /// <param name="advisedType">Type of the advised.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Internal error, can not find matching introduced field</exception>
        internal static FieldInfo FindIntroducedField(IAdvice advice, MemberInfo adviceMemberInfo, Type advisedType)
        {
            var introducedFieldType = GetIntroducedType(adviceMemberInfo);
            var adviceType = advice.GetType();
            var introducedFieldName = IntroductionRules.GetName(adviceType.Namespace, adviceType.Name, adviceMemberInfo.Name);
            var linkID = string.Format("{0}:{1}", adviceType.AssemblyQualifiedName, adviceMemberInfo.Name);
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var introducedField = FindIntroducedFieldByName(advisedType, introducedFieldName, linkID, bindingFlags)
                     ?? FindIntroducedFieldByTypeAndAvailability(advisedType, introducedFieldType, adviceMemberInfo.IsStatic(), null, bindingFlags)
                     ?? FindIntroducedFieldByTypeAndAvailability(advisedType, introducedFieldType, adviceMemberInfo.IsStatic(), linkID, bindingFlags);
            if (introducedField == null)
                throw new InvalidOperationException("Internal error, can not find matching introduced field");
            var introducedFieldAttribute = introducedField.GetAttributes<IntroducedFieldAttribute>().Single();
            introducedFieldAttribute.LinkID = linkID;
            return introducedField;
        }

        /// <summary>
        /// Determines whether the specified member type is introduction.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns></returns>
        private static bool IsIntroduction(MemberInfo memberInfo)
        {
            return GetIntroducedType(memberInfo) != null;
        }

        /// <summary>
        /// Determines whether the specified member type is introduction.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns></returns>
        private static Type GetIntroducedType(MemberInfo memberInfo)
        {
            var memberType = memberInfo.GetMemberType();
            if (!memberType.GetInformationReader().IsGenericType || memberType.GetAssignmentReader().GetGenericTypeDefinition() != typeof(IntroducedField<>))
                return null;
            return memberType.GetAssignmentReader().GetGenericArguments()[0];
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
            var introducedField = advisedType.GetMembersReader().GetField(introducedFieldName, bindingFlags);
            if (introducedField == null)
                return null;
            var introducedFieldAttribute = introducedField.GetAttributes<IntroducedFieldAttribute>().Single();
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
        /// <param name="linkID">The link identifier.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <returns></returns>
        private static FieldInfo FindIntroducedFieldByTypeAndAvailability(Type advisedType, Type fieldType, bool isStatic, string linkID, BindingFlags bindingFlags)
        {
            return (from fieldInfo in advisedType.GetMembersReader().GetFields(bindingFlags)
                    where fieldInfo.FieldType == fieldType
                          && fieldInfo.IsStatic == isStatic
                    let introducedFieldAttribute = fieldInfo.GetAttributes<IntroducedFieldAttribute>().Single()
                    where introducedFieldAttribute != null
                          && introducedFieldAttribute.LinkID == linkID
                    select fieldInfo).FirstOrDefault();
        }
    }
}
