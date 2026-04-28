#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	internal class WatchAddList<T> : IList<T>
	{
		readonly Action<List<T>> _onAdd;
		readonly List<T> _internalList;

		public WatchAddList(Action<List<T>> onAdd)
		{
			_onAdd = onAdd;
			_internalList = new List<T>();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_internalList).GetEnumerator();
		}

		public void Add(T item)
		{
			_internalList.Add(item);
			_onAdd(_internalList);
		}

		public void Clear()
		{
			_internalList.Clear();
		}

		public bool Contains(T item)
		{
			return _internalList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_internalList.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return _internalList.Remove(item);
		}

		public int Count => _internalList.Count;

		public bool IsReadOnly => false;

		public int IndexOf(T item)
		{
			return _internalList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_internalList.Insert(index, item);
			_onAdd(_internalList);
		}

		public void RemoveAt(int index)
		{
			_internalList.RemoveAt(index);
		}

		public T this[int index]
		{
			get => _internalList[index];
			set => _internalList[index] = value;
		}
	}
}
