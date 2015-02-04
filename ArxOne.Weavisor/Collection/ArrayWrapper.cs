#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Collection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ArrayWrapper<TItem> : IList<TItem>
    {
        private readonly IList<TItem> _innerList;
        private readonly int _startIndex;
        private readonly int _length;
        private readonly IEqualityComparer<TItem> _comparer;

        public int Count
        {
            get { return _length; }
        }

        public ArrayWrapper(IList<TItem> innerList, int startIndex, int length, IEqualityComparer<TItem> comparer = null)
        {
            _innerList = innerList;
            _startIndex = startIndex;
            _length = length;
            _comparer = comparer ?? EqualityComparer<TItem>.Default;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return _innerList.Skip(_startIndex).Take(_length).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(TItem item)
        {
            return _innerList.Skip(_startIndex).Take(_length).Contains(item, _comparer);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            for (int index = _startIndex; index < _startIndex + _length; index++)
                array[arrayIndex] = _innerList[index];
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(TItem item)
        {
            for (int index = 0; index < _length; index++)
            {
                if (_comparer.Equals(_innerList[index + _startIndex], item))
                    return index;
            }
            return -1;
        }
        
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
