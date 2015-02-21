#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using System.Collections.Generic;

    /// <summary>
    /// Extensions to collections
    /// </summary>
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Adds a range.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="range">The range.</param>
        public static void AddRange<TItem>(this ICollection<TItem> collection, IEnumerable<TItem> range)
        {
            foreach (var rangeItem in range)
                collection.Add(rangeItem);
        }
    }
}
