#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Aspect
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using Introduction;
    using Utility;

    internal class AdviceInfo
    {
        /// <summary>
        /// Gets the advice (always non-null).
        /// </summary>
        /// <value>
        /// The advice.
        /// </value>
        public IAdvice Advice { get; }

        /// <summary>
        /// Gets the method advice or null if none.
        /// </summary>
        /// <value>
        /// The method advice.
        /// </value>
        public IMethodAdvice MethodAdvice => Advice as IMethodAdvice;

        /// <summary>
        /// Gets the method advice or null if none.
        /// </summary>
        /// <value>
        /// The method advice.
        /// </value>
        public IMethodAsyncAdvice AsyncMethodAdvice => Advice as IMethodAsyncAdvice;

        /// <summary>
        /// Gets the property advice or null if none.
        /// </summary>
        /// <value>
        /// The property advice.
        /// </value>
        public IPropertyAdvice PropertyAdvice => Advice as IPropertyAdvice;

        /// <summary>
        /// Gets the event advice.
        /// </summary>
        /// <value>
        /// The event advice.
        /// </value>
        public IEventAdvice EventAdvice => Advice as IEventAdvice;

        /// <summary>
        /// Gets the parameter advice, or null if none.
        /// </summary>
        /// <value>
        /// The parameter advice.
        /// </value>
        public IParameterAdvice ParameterAdvice => Advice as IParameterAdvice;

        /// <summary>
        /// Gets the index of the parameter, if any (-1 stands for return value).
        /// </summary>
        /// <value>
        /// The index of the parameter.
        /// </value>
        public int? ParameterIndex { get; }

        /// <summary>
        /// Gets the introduced fields.
        /// </summary>
        /// <value>
        /// The introduced fields.
        /// </value>
        public IList<MemberInfo> IntroducedFields { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceInfo"/> class.
        /// </summary>
        /// <param name="advice">The advice.</param>
        public AdviceInfo(IAdvice advice)
        {
            Advice = advice;
            IntroducedFields = GetIntroducedFields(advice);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceInfo"/> class, specialized for parameters or return values.
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        public AdviceInfo(IAdvice advice, int parameterIndex)
        {
            Advice = advice;
            ParameterIndex = parameterIndex;
            IntroducedFields = GetIntroducedFields(advice);
        }

        public static IList<MemberInfo> GetIntroducedFields(IAdvice advice)
        {
            const BindingFlags adviceMembersBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            return new ReadOnlyCollection<MemberInfo>(advice.GetType().GetFieldsAndProperties(adviceMembersBindingFlags).Where(IsIntroduction).ToArray());
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
            var introducedFieldType = typeof(IntroducedField<>);
            foreach (var ancestor in memberType.GetSelfAndAncestors())
            {
                if (!ancestor.GetInformationReader().IsGenericType)
                    continue;

                var genericAncestorDefinition = ancestor.GetAssignmentReader().GetGenericTypeDefinition();
                if (genericAncestorDefinition == introducedFieldType)
                    return memberType.GetAssignmentReader().GetGenericArguments()[0];
            }
            return null;
        }
    }
}
