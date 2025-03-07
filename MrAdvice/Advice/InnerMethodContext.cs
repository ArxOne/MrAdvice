﻿#region Mr. Advice
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
    using global::MrAdvice.Advice;
    using Threading;
    using Utility;

    /// <summary>
    /// Special terminal advice, which calls the final method
    /// </summary>
    public class InnerMethodContext : AdviceContext
    {
        private readonly MethodInfo _innerMethod;
        private readonly ProceedDelegate _innerMethodDelegate;

        /// <summary>
        /// Gets the name of the target.
        /// </summary>
        /// <value>
        /// The name of the target.
        /// </value>
        public override string TargetName => _innerMethod.Name;

        internal InnerMethodContext(AdviceValues adviceValues, MethodInfo innerMethod, ProceedDelegate innerMethodDelegate)
            : base(adviceValues, null)
        {
            _innerMethod = innerMethod;
            _innerMethodDelegate = innerMethodDelegate;
        }

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// Here, the inner method is called
        /// </summary>
        /// <exception cref="InvalidOperationException">context.Proceed() must not be called on advised interfaces (think about it, it does not make sense).</exception>
#if NET6_0_OR_GREATER
        [System.Diagnostics.StackTraceHidden]
#endif
        internal override Task Invoke()
        {
            if (_innerMethodDelegate is not null)
            {
                AdviceValues.ReturnValue = _innerMethodDelegate(AdviceValues.Target, AdviceValues.Arguments);
                return IsTask ? (Task)AdviceValues.ReturnValue : Tasks.Void();
            }

            // _innerMethod is null for advised interfaces (because there is no implementation)
            // the advises should not call the final method
            if (_innerMethod is null)
                throw new InvalidOperationException("context.Proceed() must not be called on advised interfaces (think about it, it does not make sense).");

            try
            {
                AdviceValues.ReturnValue = _innerMethod.Invoke(AdviceValues.Target, AdviceValues.Arguments);
                return IsTask ? (Task)AdviceValues.ReturnValue : Tasks.Void();
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException.Rethrow();
            }
        }

        private bool? _isTask;
        private bool IsTask => _isTask ??= typeof(Task).GetAssignmentReader().IsAssignableFrom(_innerMethod.ReturnType);
    }
}
