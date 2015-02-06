#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Advice
{
    /// <summary>
    /// Base class for Info Advices
    /// Info Advices are called once per distinct reflection object at assembly load
    /// </summary>
    public interface IInfoAdvice
    {
    }
}
