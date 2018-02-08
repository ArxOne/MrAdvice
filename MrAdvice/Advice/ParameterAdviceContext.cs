#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Parameter advice context, passed to parameter advisors
    /// </summary>
    [DebuggerDisplay("ParameterInfo {TargetParameter}")]
    public class ParameterAdviceContext : SyncAdviceContext
    {
        private readonly IParameterAdvice _parameterAdvice;
        private readonly int _parameterIndex;

        /// <summary>
        /// Gets the parameter information.
        /// </summary>
        /// <value>
        /// The parameter information.
        /// </value>
        public ParameterInfo TargetParameter { get; }

        /// <summary>
        /// Gets the name of the target.
        /// </summary>
        /// <value>
        /// The name of the target.
        /// </value>
        public override string TargetName => TargetParameter.Name;

        /// <summary>
        /// Gets the raw type (stripped from ref if any).
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type ParameterType { get; }

        /// <summary>
        /// Gets a value indicating whether this parameter is input.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in; otherwise, <c>false</c>.
        /// </value>
        public bool IsIn { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is output.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is out; otherwise, <c>false</c>.
        /// </value>
        public bool IsOut { get; }

        /// <summary>
        /// Gets or sets the parameter value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value
        {
            get
            {
                if (_parameterIndex >= 0)
                    return AdviceValues.Arguments[_parameterIndex];
                return AdviceValues.ReturnValue;
            }
            set
            {
                if (_parameterIndex >= 0)
                    AdviceValues.Arguments[_parameterIndex] = value;
                else
                    AdviceValues.ReturnValue = value;
            }
        }

        /// <summary>
        /// Gets the typed value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <returns></returns>
        public TValue GetValue<TValue>()
        {
            return (TValue)Value;
        }

        /// <summary>
        /// Sets the typed value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        public void SetValue<TValue>(TValue value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterAdviceContext" /> class.
        /// </summary>
        /// <param name="parameterAdvice">The parameter advice.</param>
        /// <param name="targetParameter">The parameter information.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        /// <param name="adviceValues">The advice values.</param>
        /// <param name="nextAdviceContext">The next advice context.</param>
        internal ParameterAdviceContext(IParameterAdvice parameterAdvice, ParameterInfo targetParameter, int parameterIndex,
            AdviceValues adviceValues, AdviceContext nextAdviceContext)
            : base(adviceValues, nextAdviceContext)
        {
            // input parameters must be method parameters (not return value) and not out (IsOut is exclusive)
            IsIn = parameterIndex >= 0 && !targetParameter.IsOut;
            // output parameters are return value or byref or out
            IsOut = parameterIndex == -1 || targetParameter.ParameterType.IsByRef || targetParameter.IsOut;
            TargetParameter = targetParameter;
            ParameterType = targetParameter.ParameterType.IsByRef ? targetParameter.ParameterType.GetElementType() : targetParameter.ParameterType;
            _parameterAdvice = parameterAdvice;
            _parameterIndex = parameterIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterAdviceContext"/> class.
        /// </summary>
        /// <param name="parameterAdvice">The parameter advice.</param>
        /// <param name="targetParameter">The target parameter.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        /// <param name="target">The target.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="nextAdviceContext">The next advice context.</param>
        protected ParameterAdviceContext(IParameterAdvice parameterAdvice, ParameterInfo targetParameter, int parameterIndex,
            object target, Type targetType, object[] parameters, AdviceContext nextAdviceContext)
            : this(parameterAdvice, targetParameter, parameterIndex, new AdviceValues(target, targetType, parameters), nextAdviceContext)
        { }

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// </summary>
        internal override Task Invoke()
        {
            _parameterAdvice.Advise(this);
            return null;
        }
    }
}
