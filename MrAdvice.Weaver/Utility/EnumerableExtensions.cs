#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions to IEnumerable
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the index of item, using a predicate.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The index, or -1 if not found</returns>
        public static int IndexOf<TItem>(this IEnumerable<TItem> enumerable, Predicate<TItem> selector)
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                if (selector(item))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Applies an action to given collection.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="action">The action.</param>
        public static void ForAll<TItem>(this IEnumerable<TItem> enumerable, Action<TItem> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
    }
}
