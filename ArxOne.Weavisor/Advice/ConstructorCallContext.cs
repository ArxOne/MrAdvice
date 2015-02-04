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
    public class ConstructorCallContext : CallContext
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
        /// Gets the target method.
        /// </summary>
        /// <value>
        /// The target method.
        /// </value>
        public ConstructorInfo TargetConstructor { get; private set; }

        private readonly MethodInfo _innerMethod;
        private readonly IList<IConstructorAdvice> _advices;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallContext" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="targetConstructor">The target constructor.</param>
        /// <param name="innerMethod">The inner method.</param>
        /// <param name="advices">The advices.</param>
        public ConstructorCallContext(object target, object[] parameters, ConstructorInfo targetConstructor, MethodInfo innerMethod, IList<IConstructorAdvice> advices)
        {
            _innerMethod = innerMethod;
            _advices = advices;
            Target = target;
            Parameters = parameters;
            TargetConstructor = targetConstructor;
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
                var constructorAdvice = new Call<ConstructorCallContext>(this, step);
                _advices[step].Advise(constructorAdvice);
            }
            else
                _innerMethod.Invoke(Target, Parameters);
        }
    }
}
