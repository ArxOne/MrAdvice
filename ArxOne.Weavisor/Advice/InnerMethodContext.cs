#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Advice
{
    using System.Reflection;

    /// <summary>
    /// Special terminal advice, which calls the final method
    /// </summary>
    internal class InnerMethodContext : AdviceContext
    {
        private readonly MethodInfo _innerMethod;

        public InnerMethodContext(AdviceValues adviceValues,  MethodInfo innerMethod)
            : base(adviceValues, null)
        {
            _innerMethod = innerMethod;
        }

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// </summary>
        public override void Invoke()
        {
            AdviceValues.ReturnValue = _innerMethod.Invoke(AdviceValues.Target, AdviceValues.Parameters);
        }
    }
}
