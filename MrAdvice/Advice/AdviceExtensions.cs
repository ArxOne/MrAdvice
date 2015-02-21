#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System;

    /// <summary>
    /// Extensions to IAdvice (one extension, actually)
    /// </summary>
    public static class AdviceExtensions
    {
        /// <summary>
        /// Creates a proxy around the given interface, and injects the given advice at all levels.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="advice">The advice.</param>
        /// <returns></returns>
        public static TInterface Handle<TInterface>(this IAdvice advice)
        {
            throw new NotImplementedException();
        }
    }
}
