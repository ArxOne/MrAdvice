#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
    /// <summary>
    /// Advice when weaving method
    /// </summary>
    public interface IMethodWeavingAdvice: IWeavingAdvice
    {
        /// <summary>
        /// Advises the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        void Advise(MethodWeavingContext context);
    }
}
