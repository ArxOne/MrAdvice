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
        private readonly MethodInfo _innerMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAdviceContext" /> class.
        /// </summary>
        /// <param name="adviceValues">The call values.</param>
        /// <param name="methodAdvice">The method advice.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="nextAdviceContext">The next advice context.</param>
        /// <param name="innerMethod">The inner method.</param>
        internal MethodAdviceContext(AdviceValues adviceValues, IMethodAdvice methodAdvice, MethodBase targetMethod, AdviceContext nextAdviceContext, MethodInfo innerMethod)
            : base(adviceValues, nextAdviceContext)
        {
            _methodAdvice = methodAdvice;
            _innerMethod = innerMethod;
            TargetMethod = targetMethod;
        }

        public override void Invoke()
        {
            if (_methodAdvice != null)
                _methodAdvice.Advise(this);
            else
                ReturnValue = _innerMethod.Invoke(Target, Parameters);
        }
    }
}