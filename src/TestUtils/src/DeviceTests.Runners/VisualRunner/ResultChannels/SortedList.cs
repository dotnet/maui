using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Utilities
{
    class SortedList<T> : IList<T>
    {
        readonly IComparer<T> comparer;
        readonly List<T> list = new List<T>();

        public SortedList(IComparer<T> comparer)
        {
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public int IndexOf(T item)
        {
            // PERF hint: this is a O(n) algorithm but could be rewritten as a O(log n) one.
            if (Count == 0)
            {
                return ~0;
            }

            for (var i = 0; i < Count; i++)
            {
                var existing = this[i];
                var compare = comparer.Compare(item, existing);
                if (compare == 0)
                {
                    return i;
                }
                if (compare < 0)
                {
                    return ~i;
                }
            }

            return ~Count;
        }

        public void Insert(int index, T item)
        {
            // We trust our caller to be passing in a sorted index.
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return list[index]; }
            set { throw new NotSupportedException(); }
        }

        public void Add(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                index = ~index;
            }

            list.Insert(index, item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}