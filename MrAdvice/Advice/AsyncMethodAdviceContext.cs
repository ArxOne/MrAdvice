namespace ArxOne.MrAdvice.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using Threading;

    /// <summary>
    /// Method advice context, passed to method advisors
    /// </summary>
    [DebuggerDisplay("async MethodInfo {TargetMethod}")]
    public class AsyncMethodAdviceContext : AdviceContext
    {
        /// <summary>
        /// Gets the parameters.
        /// Each parameter can be individually changed before Call.Proceed()
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IList<object> Parameters => AdviceValues.Parameters;

        /// <summary>
        /// Gets a value indicating whether the advised method has a return value.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has return value; otherwise, <c>false</c>.
        /// </value>
        public bool HasReturnValue
        {
            get
            {
                var methodInfo = TargetMethod as MethodInfo;
                if (methodInfo == null) // ctor
                    return false;
                return methodInfo.ReturnType != typeof(void);
            }
        }

        /// <summary>
        /// Gets or sets the return value (after Call.Proceed()).
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        /// <exception cref="InvalidOperationException" accessor="get">Method has no ReturnValue</exception>
        /// <exception cref="InvalidOperationException" accessor="set">Method has no ReturnValue</exception>
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
        /// Gets the target method.
        /// </summary>
        /// <value>
        /// The target method.
        /// </value>
        public MethodBase TargetMethod { get; }

        private readonly IAsyncMethodAdvice _methodAdvice;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAdviceContext" /> class.
        /// </summary>
        /// <param name="methodAdvice">The method advice.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="adviceValues">The call values.</param>
        /// <param name="nextAdviceContext">The next advice context.</param>
        internal AsyncMethodAdviceContext(IAsyncMethodAdvice methodAdvice, MethodBase targetMethod, AdviceValues adviceValues, AdviceContext nextAdviceContext)
            : base(adviceValues, nextAdviceContext)
        {
            _methodAdvice = methodAdvice;
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// Proceeds to the next advice, asynchronously
        /// </summary>
        public Task ProceedAsync() => InvokeNext() ?? Tasks.Void();

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// </summary>
        internal override Task Invoke()
        {
            return _methodAdvice.Advise(this);
        }
    }
}