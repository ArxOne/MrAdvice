#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Introduction
{
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
        private readonly FieldInfo _introducedField;
        private bool IsStatic { get { return _introducedField.IsStatic; } }

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
            get { return (TFieldType)_introducedField.GetValue(IsStatic ? null : context.Target); }
            set { _introducedField.SetValue(IsStatic ? null : context.Target, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroducedField{TFieldType}"/> class.
        /// </summary>
        /// <param name="introducedField">The introduced field.</param>
        public IntroducedField(FieldInfo introducedField)
        {
            _introducedField = introducedField;
        }
    }
}
