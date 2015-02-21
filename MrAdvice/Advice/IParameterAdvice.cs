#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Advices implementing this interface work on parameters.
    /// Can be used at assembly, type, method, property or parameter scope
    /// </summary>
    public interface IParameterAdvice : IAdvice
    {
        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The parameter advice context.</param>
        void Advise(ParameterAdviceContext context);
    }
}
