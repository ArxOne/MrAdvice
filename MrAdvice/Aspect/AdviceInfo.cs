#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Aspect
{
    using Advice;

    internal class AdviceInfo
    {
        /// <summary>
        /// Gets the advice (always non-null).
        /// </summary>
        /// <value>
        /// The advice.
        /// </value>
        public IAdvice Advice { get; private set; }
        /// <summary>
        /// Gets the method advice or null if none.
        /// </summary>
        /// <value>
        /// The method advice.
        /// </value>
        public IMethodAdvice MethodAdvice { get { return Advice as IMethodAdvice; } }
        /// <summary>
        /// Gets the property advice or null if none.
        /// </summary>
        /// <value>
        /// The property advice.
        /// </value>
        public IPropertyAdvice PropertyAdvice { get { return Advice as IPropertyAdvice; } }
        /// <summary>
        /// Gets the parameter advice, or null if none.
        /// </summary>
        /// <value>
        /// The parameter advice.
        /// </value>
        public IParameterAdvice ParameterAdvice { get { return Advice as IParameterAdvice; } }
        /// <summary>
        /// Gets the index of the parameter, if any (-1 stands for return value).
        /// </summary>
        /// <value>
        /// The index of the parameter.
        /// </value>
        public int? ParameterIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceInfo"/> class.
        /// </summary>
        /// <param name="advice">The advice.</param>
        public AdviceInfo(IAdvice advice)
        {
            Advice = advice;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceInfo"/> class, specialized for parameters or return values.
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        public AdviceInfo(IAdvice advice, int parameterIndex)
        {
            Advice = advice;
            ParameterIndex = parameterIndex;
        }
    }
}