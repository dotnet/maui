using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xamarin.Forms
{
	internal class SynchronizedList<T> : IList<T>, IReadOnlyList<T>
	{
		readonly List<T> _list = new List<T>();
		ReadOnlyCollection<T> _snapshot;

		public void Add(T item)
		{
			lock (_list)
			{
				_list.Add(item);
				_snapshot = null;
			}
		}

		public void Clear()
		{
			lock (_list)
			{
				_list.Clear();
				_snapshot = null;
			}
		}

		public bool Contains(T item)
		{
			lock (_list)
				return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_list)
				_list.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _list.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(T item)
		{
			lock (_list)
			{
				if (_list.Remove(item))
				{
					_snapshot = null;
					return true;
				}

				return false;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			ReadOnlyCollection<T> snap = _snapshot;
			if (snap == null)
			{
				lock (_list)
					_snapshot = snap = new ReadOnlyCollection<T>(_list.ToList());
			}

			return snap.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			lock (_list)
				return _list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			lock (_list)
			{
				_list.Insert(index, item);
				_snapshot = null;
			}
		}

		public T this[int index]
		{
			get
			{
				ReadOnlyCollection<T> snap = _snapshot;
				if (snap != null)
					return snap[index];

				lock (_list)
					return _list[index];
			}

			set
			{
				lock (_list)
				{
					_list[index] = value;
					_snapshot = null;
				}
			}
		}

		public void RemoveAt(int index)
		{
			lock (_list)
			{
				_list.RemoveAt(index);
				_snapshot = null;
			}
		}
	}
}