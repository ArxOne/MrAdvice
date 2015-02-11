#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System.Reflection;

    /// <summary>
    /// Advices for <see cref="MethodInfo"/>
    /// </summary>
    public interface IMethodInfoAdvice: IInfoAdvice
    {
        /// <summary>
        /// Invoked once per method, when assembly is loaded
        /// </summary>
        /// <param name="context">The method info advice context</param>
        void Advise(MethodInfoAdviceContext context);
    }
}
