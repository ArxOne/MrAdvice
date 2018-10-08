#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Introduction
{
    using System;
    using System.Reflection;
    using Advice;

    /// <summary>
    /// This class allows to introduce fields in advised type
    /// To use it, declare instances of it in advice,
    /// then use the indexer to access introduced field in advised type instance
    /// This field (the default) introduces one field per advice, regardless the number of advices with the same type.
    /// It can be considered as shared between all advices to the same type
    /// </summary>
    /// <typeparam name="TField">The type of the field.</typeparam>
    public class SharedIntroducedField<TField> : IntroducedField<TField>
    {
        /// <inheritdoc />
        protected override bool IsShared => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedIntroducedField{TField}"/> class.
        /// </summary>
        /// <param name="ownerAdvice">The owner advice.</param>
        /// <param name="ownerMemberInfo">The owner member information.</param>
        [Obsolete("Let Mr. Advice use it, don't bother instantiating it by yourself.")]
        public SharedIntroducedField(IAdvice ownerAdvice, MemberInfo ownerMemberInfo)
            : base(ownerAdvice, ownerMemberInfo)
        {
        }
    }
}
