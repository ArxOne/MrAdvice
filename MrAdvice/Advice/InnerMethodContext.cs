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

    /// <summary>
    /// Special terminal advice, which calls the final method
    /// </summary>
    internal class InnerMethodContext : AdviceContext
    {
        private readonly MethodInfo _innerMethod;

        public InnerMethodContext(AdviceValues adviceValues, MethodInfo innerMethod)
            : base(adviceValues, null)
        {
            _innerMethod = innerMethod;
        }

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// Here, the inner method is called
        /// </summary>
        internal override void Invoke()
        {
            // _innerMethod is null for advised interfaces (because there is no implementation)
            // the advises should not call the final method
            if (_innerMethod == null)
                throw new InvalidOperationException("context.Proceed() must not be call on advised interfaces (think about it, it does not make sense).");

            try
            {
                //var delegateType = Expression.GetDelegateType(
                //    _innerMethod.GetParameters().Select(p => p.ParameterType).Concat(new[] { _innerMethod.ReturnType }).ToArray());
                //var d = Delegate.CreateDelegate(delegateType, AdviceValues.Target, _innerMethod);
                //AdviceValues.ReturnValue = d.DynamicInvoke(AdviceValues.Parameters);
                AdviceValues.ReturnValue = _innerMethod.Invoke(AdviceValues.Target, AdviceValues.Parameters);
            }
            catch (TargetInvocationException tie)
            {
                var ie = tie.InnerException;
                var p = typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null)
                    p.Invoke(ie, new object[0]);
                throw ie;
            }
        }
    }
}
