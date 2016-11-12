#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
    using System.Threading.Tasks;

    /// <summary>
    /// Advices implementing this interface work on async methods.
    /// Can be used at assembly, type, method or property scope
    /// </summary>
    public interface IMethodAsyncAdvice: IAdvice
    {
        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        Task Advise(MethodAsyncAdviceContext context);
    }
}
