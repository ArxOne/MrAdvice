#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System.Collections.Generic;
    using dnlib.DotNet;

    /// <summary>
    /// Type comparer for <see cref="Dictionary{TKey,TValue}"/> and friends
    /// </summary>
    public class TypeComparer : IEqualityComparer<ITypeDefOrRef>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(ITypeDefOrRef x, ITypeDefOrRef y)
        {
            return x.FullName == y.FullName;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(ITypeDefOrRef obj)
        {
            return obj.FullName.GetHashCode();
        }

        /// <summary>
        /// The instance
        /// </summary>
        public static TypeComparer Instance = new TypeComparer();
    }
}
