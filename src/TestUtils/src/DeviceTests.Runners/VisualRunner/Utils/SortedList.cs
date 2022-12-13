#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class SortedList<T> : IList<T>
	{
		readonly IComparer<T> _comparer;
		readonly List<T> _list;

		public SortedList(IComparer<T> comparer)
		{
			_comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
			_list = new List<T>();
		}

		public int IndexOf(T item)
		{
			if (Count == 0)
				return ~0;

			return _list.BinarySearch(item, _comparer);
		}

		public void Insert(int index, T item)
		{
			// We trust our caller to be passing in a sorted index.
			_list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		public T this[int index]
		{
			get => _list[index];
			set => throw new NotSupportedException();
		}

		public void Add(T item)
		{
			var index = IndexOf(item);
			if (index < 0)
			{
				index = ~index;
			}

			_list.Insert(index, item);
		}

		public void Clear()
		{
			_list.Clear();
		}

		public bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		public int Count => _list.Count;

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

		public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}