#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Threading;
    using Utility;

    /// <summary>
    /// Special terminal advice, which calls the final method
    /// </summary>
    public class InnerMethodContext : AdviceContext
    {
        private readonly MethodInfo _innerMethod;

        internal InnerMethodContext(AdviceValues adviceValues, MethodInfo innerMethod)
            : base(adviceValues, null)
        {
            _innerMethod = innerMethod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InnerMethodContext"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="innerMethod">The inner method.</param>
        protected InnerMethodContext(object target, Type targetType, object[] parameters, MethodInfo innerMethod)
            : this(new AdviceValues(target, targetType, parameters), innerMethod)
        { }

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// Here, the inner method is called
        /// </summary>
        /// <exception cref="InvalidOperationException">context.Proceed() must not be called on advised interfaces (think about it, it does not make sense).</exception>
        internal override Task Invoke()
        {
            // _innerMethod is null for advised interfaces (because there is no implementation)
            // the advises should not call the final method
            if (_innerMethod == null)
                throw new InvalidOperationException("context.Proceed() must not be called on advised interfaces (think about it, it does not make sense).");

            try
            {
                AdviceValues.ReturnValue = _innerMethod.Invoke(AdviceValues.Target, AdviceValues.Arguments);
                if (typeof(Task).GetAssignmentReader().IsAssignableFrom(_innerMethod.ReturnType))
                    return (Task)AdviceValues.ReturnValue;
                return Tasks.Void();
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException.PreserveStackTrace();
            }
        }
    }
}
