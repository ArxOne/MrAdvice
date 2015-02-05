#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Initializer
{
    using System.Reflection;

    /// <summary>
    /// Runtime initialization for methods
    /// </summary>
    public interface IMethodInitializer: IInitializer
    {
        /// <summary>
        /// Invoked once per method, when assembly is loaded
        /// </summary>
        /// <param name="methodBase"></param>
        void Initialize(MethodBase methodBase);
    }
}
