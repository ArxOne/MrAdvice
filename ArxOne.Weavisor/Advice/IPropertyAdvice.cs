#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Advice
{
    /// <summary>
    /// Advices implementing this interface can intercept access to properties
    /// In a more precise way than IMethodAdvice would
    /// </summary>
    public interface IPropertyAdvice: IAdvice
    {
        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        void Advise(PropertyAdviceContext context);
    }
}
