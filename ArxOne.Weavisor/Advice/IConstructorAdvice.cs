#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Advice
{
    /// <summary>
    /// Advices implementing this interface work on constructors.
    /// Can be used at assembly, type, method or property scope
    /// </summary>
    public interface IConstructorAdvice: IAdvice
    {
        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke call.Proceed()
        /// </summary>
        /// <param name="call">The call.</param>
        void Advise(Call<ConstructorCallContext> call);
    }
}
