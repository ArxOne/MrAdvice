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
    }
}
