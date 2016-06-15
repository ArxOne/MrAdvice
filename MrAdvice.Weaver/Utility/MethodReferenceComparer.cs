namespace ArxOne.MrAdvice.Utility
{
    using System.Collections.Generic;
    using dnlib.DotNet;

    /// <summary>
    /// Equality comparer for method references
    /// </summary>
    internal class MethodReferenceComparer : IEqualityComparer<MethodDef>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <cref name="MethodReference" /> to compare.</param>
        /// <param name="y">The second object of type <cref name="MethodReference" /> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(MethodDef x, MethodDef y)
        {
            return x.SafeEquivalent(y, true);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(MethodDef obj)
        {
            return obj.DeclaringType.FullName.GetHashCode() ^ obj.Name.GetHashCode();
        }
    }
}