#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Base class for Info Advices
    /// Info Advices are called once per distinct reflection object at assembly load
    /// </summary>
    public interface IInfoAdvice
    {
    }
}
