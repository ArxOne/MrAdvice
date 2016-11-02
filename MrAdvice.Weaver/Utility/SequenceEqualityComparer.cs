#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System.Collections.Generic;
    using System.Linq;

    public class SequenceEqualityComparer<TItem> : IEqualityComparer<TItem[]>
    {
        private readonly IEqualityComparer<TItem> _comparer = EqualityComparer<TItem>.Default;

        public bool Equals(TItem[] x, TItem[] y)
        {
            return x.SequenceEqual(y, _comparer);
        }

        public int GetHashCode(TItem[] obj)
        {
            return obj.Aggregate(0, (h, i) => h ^ i.GetHashCode());
        }
    }
}
