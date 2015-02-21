#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Advices implementing this interface work on methods.
    /// Can be used at assembly, type, method or property scope
    /// </summary>
    public interface IMethodAdvice: IAdvice
    {
        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        void Advise(MethodAdviceContext context);
    }
}
