#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Advice
{
    using System.Reflection;

    public class MethodAdviceContext : AdviceContext
    {
        /// <summary>
        /// Gets the parameters.
        /// Each parameter can be individually changed before Call.Proceed()
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public object[] Parameters { get { return AdviceValues.Parameters; } }
        /// <summary>
        /// Gets or sets the return value (after Call.Proceed()).
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        public object ReturnValue { get { return AdviceValues.ReturnValue; } set { AdviceValues.ReturnValue = value; } }
        /// <summary>
        /// Gets the target method.
        /// </summary>
        /// <value>
        /// The target method.
        /// </value>
        public MethodBase TargetMethod { get; private set; }

        private readonly IMethodAdvice _methodAdvice;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAdviceContext" /> class.
        /// </summary>
        /// <param name="methodAdvice">The method advice.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="adviceValues">The call values.</param>
        /// <param name="nextAdviceContext">The next advice context.</param>
        internal MethodAdviceContext(IMethodAdvice methodAdvice, MethodBase targetMethod, AdviceValues adviceValues, AdviceContext nextAdviceContext)
            : base(adviceValues, nextAdviceContext)
        {
            _methodAdvice = methodAdvice;
            TargetMethod = targetMethod;
        }

        public override void Invoke()
        {
            _methodAdvice.Advise(this);
        }
    }
}