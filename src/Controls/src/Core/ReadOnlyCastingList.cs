#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	internal class ReadOnlyCastingList<T, TFrom> : IReadOnlyList<T> where T : class where TFrom : class
	{
		readonly IList<TFrom> _list;

		public ReadOnlyCastingList(IList<TFrom> list)
		{
			_list = list;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_list).GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new CastingEnumerator<T, TFrom>(_list.GetEnumerator());
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public T this[int index]
		{
			get { return _list[index] as T; }
		}
	}

	internal class ReadOnlyCastingReadOnlyList<T, TFrom> : IReadOnlyList<T> where T : class where TFrom : class
	{
		readonly IReadOnlyList<TFrom> _readonlyList;

		public ReadOnlyCastingReadOnlyList(IReadOnlyList<TFrom> list)
		{
			_readonlyList = list;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_readonlyList).GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new CastingEnumerator<T, TFrom>(_readonlyList.GetEnumerator());
		}

		public int Count
		{
			get { return _readonlyList.Count; }
		}

		public T this[int index]
		{
			get { return _readonlyList[index] as T; }
		}
	}

	class CastingList<T, TFrom> : IList<T>
		where T : class
		where TFrom : class
	{
		readonly IList<TFrom> _list;

		public CastingList(IList<TFrom> list)
		{
			_list = list;
		}

		public T this[int index]
		{
			get => _list[index] as T;
			set => _list[index] = value as TFrom;
		}

		public int Count => _list.Count;

		public bool IsReadOnly => _list.IsReadOnly;

		public void Add(T item) =>
			_list.Add(item as TFrom);

		public void Clear() => _list.Clear();

		public bool Contains(T item)
			=> _list.Contains(item as TFrom);

		public void CopyTo(T[] array, int arrayIndex)
		{
			for (int i = arrayIndex; i < array.Length; i++)
			{
				array[i] = _list[i] as T;
			}
		}

		public IEnumerator<T> GetEnumerator() =>
			new CastingEnumerator<T, TFrom>(_list.GetEnumerator());

		public int IndexOf(T item) =>
			_list.IndexOf(item as TFrom);

		public void Insert(int index, T item) =>
			_list.Insert(index, item as TFrom);

		public bool Remove(T item) =>
			_list.Remove(item as TFrom);

		public void RemoveAt(int index) =>
			_list.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_list).GetEnumerator();
		}
	}
}