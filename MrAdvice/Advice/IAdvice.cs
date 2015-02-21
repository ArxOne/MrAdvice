#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Represents an advice; this is the base marker for all advices
    /// You don't have to explicitly implement this interface, since all other interfaces inherit it
    /// </summary>
    public interface IAdvice
    {
    }
}
