#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Introduction
{
    using System.Diagnostics.CodeAnalysis;
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

        /// <summary>
        /// Gets or sets the <see cref="TFieldType"/> with the specified context.
        /// </summary>
        /// <value>
        /// The <see cref="TFieldType"/>.
        /// </value>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public TFieldType this[AdviceContext context]
        {
            get { return (TFieldType)_introducedField.GetValue(context.Target); }
            set { _introducedField.SetValue(context.Target, value); }
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
