#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Collection;

    /// <summary>
    /// Property advice context, passed to property advisors
    /// </summary>
    [DebuggerDisplay("Property: {TargetProperty}, {DebuggerGetSet}")]
    public class PropertyAdviceContext : AdviceContext
    {
        /// <summary>
        /// Gets the index for property.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public IList<object> Index { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the property call has a value (is a setter, actually).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has value; otherwise, <c>false</c>.
        /// </value>
        public bool HasValue { get { return IsSetter; } }

        /// <summary>
        /// Gets or sets the property value (for setters only).
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <exception cref="System.InvalidOperationException">
        /// Method has no Value
        /// or
        /// Method has no Value
        /// </exception>
        public object Value
        {
            get
            {
                if (!HasValue)
                    throw new InvalidOperationException("Method has no Value");
                return AdviceValues.Parameters[0];
            }
            set
            {
                if (!HasValue)
                    throw new InvalidOperationException("Method has no Value");
                AdviceValues.Parameters[0] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the property has return value (is a getter, actually).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has return value; otherwise, <c>false</c>.
        /// </value>
        public bool HasReturnValue { get { return IsGetter; } }

        /// <summary>
        /// Gets or sets the return value (after Proceed()).
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        public object ReturnValue
        {
            get
            {
                if (!HasReturnValue)
                    throw new InvalidOperationException("Method has no ReturnValue");
                return AdviceValues.ReturnValue;
            }
            set
            {
                if (!HasReturnValue)
                    throw new InvalidOperationException("Method has no ReturnValue");
                AdviceValues.ReturnValue = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this context is a getter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is getter; otherwise, <c>false</c>.
        /// </value>
        public bool IsGetter { get { return !IsSetter; } }

        /// <summary>
        /// Gets a value indicating whether this context is a setter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is setter; otherwise, <c>false</c>.
        /// </value>
        public bool IsSetter { get; private set; }

        private string DebuggerGetSet{get { return IsGetter ? "Getter" : "Setter"; }}

        /// <summary>
        /// Gets the target property.
        /// </summary>
        /// <value>
        /// The target property.
        /// </value>
        public PropertyInfo TargetProperty { get; private set; }

        private readonly IPropertyAdvice _propertyAdvice;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAdviceContext"/> class.
        /// </summary>
        /// <param name="propertyAdvice">The property advice.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="isSetter">if set to <c>true</c> [is setter].</param>
        /// <param name="adviceValues">The advice values.</param>
        /// <param name="nextAdviceContext">The next advice context.</param>
        /// <exception cref="System.InvalidOperationException">Only properties can be advised with this interface</exception>
        internal PropertyAdviceContext(IPropertyAdvice propertyAdvice, PropertyInfo propertyInfo, bool isSetter, AdviceValues adviceValues, AdviceContext nextAdviceContext)
            : base(adviceValues, nextAdviceContext)
        {
            _propertyAdvice = propertyAdvice;
            TargetProperty = propertyInfo;
            IsSetter = isSetter;
            if (IsGetter)
                Index = new ArraySpan<object>(AdviceValues.Parameters, 0, AdviceValues.Parameters.Length);
            else
                Index = new ArraySpan<object>(AdviceValues.Parameters, 1, AdviceValues.Parameters.Length - 1);
        }

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// </summary>
        internal override void Invoke()
        {
            _propertyAdvice.Advise(this);
        }
    }
}
