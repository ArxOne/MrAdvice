#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Introduction
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Advice;

    /// <summary>
    /// This class allows to introduce fields in advised type
    /// To use it, declare instances of it in advice,
    /// then use the indexer to access introduced field in advised type instance
    /// </summary>
    /// <typeparam name="TFieldType">The type of the field type.</typeparam>
    public class IntroducedField<TFieldType>
    {
        private readonly IAdvice _ownerAdvice;
        private readonly MemberInfo _ownerMemberInfo;

        private readonly IDictionary<Type, FieldInfo> _fieldInfos = new Dictionary<Type, FieldInfo>();

        /// <summary>
        /// Gets the introduced field.
        /// Since the attribute may be at assembly level, the advised type is given as parameter
        /// A cache is kept, by target type.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        private FieldInfo GetIntroducedField(Type targetType)
        {
            lock (_fieldInfos)
            {
                FieldInfo introducedField;
                if (_fieldInfos.TryGetValue(targetType, out introducedField))
                    return introducedField;

                _fieldInfos[targetType] = introducedField = Invocation.FindIntroducedField(_ownerAdvice, _ownerMemberInfo, targetType);
                return introducedField;
            }
        }
        /// <summary>
        /// Gets or sets the <see typeparamref="TFieldType"/> with the specified context.
        /// </summary>
        /// <value>
        /// The <see typeparamref="TFieldType"/>.
        /// </value>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public TFieldType this[IAdviceContextTarget context]
        {
            get
            {
                var introducedField = GetIntroducedField(context.TargetType);
                return (TFieldType)introducedField.GetValue(context.Target);
            }
            set
            {
                var introducedField = GetIntroducedField(context.TargetType);
                introducedField.SetValue(context.Target, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroducedField{TFieldType}" /> class.
        /// </summary>
        /// <param name="ownerAdvice">The owner advice.</param>
        /// <param name="ownerMemberInfo">The owner member information.</param>
        [Obsolete("Let Mr. Advice use it, don't bother instantiating it by yourself.")]
        public IntroducedField(IAdvice ownerAdvice, MemberInfo ownerMemberInfo)
        {
            _ownerAdvice = ownerAdvice;
            _ownerMemberInfo = ownerMemberInfo;
        }
    }
}
