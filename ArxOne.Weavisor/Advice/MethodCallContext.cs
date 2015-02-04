#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Advice
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Call context for method advices
    /// </summary>
    public class MethodCallContext : CallContext
    {
        /// <summary>
        /// Gets or sets the target (the instance to which the advice applies).
        /// null for static methods
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public object Target { get; set; }
        /// <summary>
        /// Gets the parameters.
        /// Each parameter can be individually changed before Call.Proceed()
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public object[] Parameters { get; private set; }
        /// <summary>
        /// Gets or sets the return value (after Call.Proceed()).
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        public object ReturnValue { get; set; }
        /// <summary>
        /// Gets the target method.
        /// </summary>
        /// <value>
        /// The target method.
        /// </value>
        public MethodBase TargetMethod { get; private set; }

        private readonly MethodInfo _innerMethod;
        private readonly IList<IMethodAdvice> _advices;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallContext"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="innerMethod">The inner method.</param>
        /// <param name="advices">The advices.</param>
        public MethodCallContext(object target, object[] parameters, MethodBase targetMethod, MethodInfo innerMethod, IList<IMethodAdvice> advices)
        {
            _innerMethod = innerMethod;
            _advices = advices;
            Target = target;
            Parameters = parameters;
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// Proceeds to specified step (next step).
        /// Internally used by Call
        /// </summary>
        /// <param name="step">The step.</param>
        internal override void Proceed(int step)
        {
            if (step < _advices.Count)
            {
                var methodAdvice = new Call<MethodCallContext>(this, step);
                _advices[step].Advise(methodAdvice);
            }
            else
                ReturnValue = _innerMethod.Invoke(Target, Parameters);
        }
    }
}
