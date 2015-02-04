#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Advice
{
    /// <summary>
    /// Advices implementing this interface work on methods.
    /// Can be used at assembly, type, method or property scope
    /// </summary>
    public interface IMethodAdvice: IAdvice
    {
        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke call.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        void Advise(MethodAdviceContext context);
    }
}
