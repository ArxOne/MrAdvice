#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Collection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Array span, based on inner arrary with start index and length
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    internal class ArraySpan<TItem> : IList<TItem>
    {
        private readonly IList<TItem> _innerList;
        private readonly int _startIndex;
        private readonly int _length;
        private readonly IEqualityComparer<TItem> _comparer;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArraySpan{TItem}"/> class.
        /// </summary>
        /// <param name="innerList">The inner list, wrapped by the instance.</param>
        /// <param name="startIndex">The start index (will become 0 here).</param>
        /// <param name="length">The new length.</param>
        /// <param name="comparer">A comparer, or null to use default.</param>
        public ArraySpan(IList<TItem> innerList, int startIndex, int length, IEqualityComparer<TItem> comparer = null)
        {
            _innerList = innerList;
            _startIndex = startIndex;
            _length = length;
            _comparer = comparer ?? EqualityComparer<TItem>.Default;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TItem> GetEnumerator() => _innerList.Skip(_startIndex).Take(_length).GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(TItem item) => _innerList.Skip(_startIndex).Take(_length).Contains(item, _comparer);

        /// <summary>
        /// Copies to the target array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            for (int index = _startIndex; index < _startIndex + _length; index++)
                array[arrayIndex] = _innerList[index];
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(TItem item)
        {
            for (int index = 0; index < _length; index++)
            {
                if (_comparer.Equals(_innerList[index + _startIndex], item))
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public TItem this[int index]
        {
            get { return _innerList[_startIndex + index]; }
            set { _innerList[_startIndex + index] = value; }
        }

        #region Invalid write operations

        void ICollection<TItem>.Add(TItem item)
        {
            throw new InvalidOperationException("Add() is not supported");
        }

        void IList<TItem>.Insert(int index, TItem item)
        {
            throw new InvalidOperationException("Insert() is not supported");
        }

        bool ICollection<TItem>.Remove(TItem item)
        {
            throw new InvalidOperationException("Remove() is not supported");
        }

        void IList<TItem>.RemoveAt(int index)
        {
            throw new InvalidOperationException("RemoveAt() is not supported");
        }

        void ICollection<TItem>.Clear()
        {
            throw new InvalidOperationException("Clear() is not supported");
        }

        #endregion
    }
}
