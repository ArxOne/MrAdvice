using System;
using System.Collections.Generic;
using System.Text;

namespace ArxOne.MrAdvice.Advice
{
    internal class AdviceComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            // Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            // Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;


            return x.GetType().Equals(y.GetType());
        }

        public int GetHashCode(T advice)
        {
            // Check whether the object is null
            if (Object.ReferenceEquals(advice, null)) return 0;

            // Get the hash code for the Id field if it is not null.
            return advice.GetType().GetHashCode();
        }

    }
}
