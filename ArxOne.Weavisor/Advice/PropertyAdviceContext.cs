#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Advice
{
    using System;
    using System.Reflection;

    public class PropertyAdviceContext : AdviceContext
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public object[] Index { get { return AdviceValues.Parameters; } }

        /// <summary>
        /// Gets or sets the return value (after Proceed()).
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        public object ReturnValue { get { return AdviceValues.ReturnValue; } set { AdviceValues.ReturnValue = value; } }

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
        }

        public override void Invoke()
        {
            _propertyAdvice.Advise(this);
        }
    }
}
