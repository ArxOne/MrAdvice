#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Advice context base class
    /// </summary>
    public abstract class AdviceContext : IAdviceContextTarget
    {
        private readonly AdviceContext _nextAdviceContext;

        /// <summary>
        /// Advice values are shared between advices.
        /// They are:
        /// - parameters
        /// - return value
        /// </summary>
        /// <value>
        /// The advice values.
        /// </value>
        internal AdviceValues AdviceValues { get; private set; }

        /// <summary>
        /// Gets or sets the target (the instance to which the advice applies).
        /// null for static methods
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public object Target { get { return AdviceValues.Target; } set { AdviceValues.Target = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceContext" /> class.
        /// </summary>
        /// <param name="adviceValues">The advice values.</param>
        /// <param name="nextAdviceContext">The next advice context.</param>
        internal AdviceContext(AdviceValues adviceValues, AdviceContext nextAdviceContext)
        {
            _nextAdviceContext = nextAdviceContext;
            AdviceValues = adviceValues;
        }

        /// <summary>
        /// Proceeds to the next advice
        /// </summary>
        public void Proceed()
        {
            _nextAdviceContext.Invoke();
        }

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// </summary>
        internal abstract void Invoke();
    }
}
