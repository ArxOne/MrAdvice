#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Advice
{
    /// <summary>
    /// Contains all information related to current call
    /// </summary>
    public abstract class CallContext
    {
        /// <summary>
        /// Proceeds to specified step (next step).
        /// Internally used by Call
        /// </summary>
        /// <param name="step">The step.</param>
        internal abstract void Proceed(int step);
    }
}