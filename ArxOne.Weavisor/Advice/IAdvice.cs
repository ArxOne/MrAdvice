#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Advice
{
    /// <summary>
    /// Represents an advice; this is the base marker for all advices
    /// You don't have to explicitly implement this interface, since all other interfaces inherit it
    /// </summary>
    public interface IAdvice
    {
    }
}
