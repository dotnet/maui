using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	internal sealed class ReadOnlyListAdapter : IList
	{
		readonly IReadOnlyCollection<object> _collection;
		readonly IReadOnlyList<object> _list;

		public ReadOnlyListAdapter(IReadOnlyList<object> list)
		{
			_list = list;
			_collection = list;
		}

		public ReadOnlyListAdapter(IReadOnlyCollection<object> collection)
		{
			_collection = collection;
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return _collection.Count; }
		}

		public bool IsSynchronized
		{
			get { throw new NotImplementedException(); }
		}

		public object SyncRoot
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerator GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		public int Add(object value)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(object value)
		{
			return _list.Contains(value);
		}

		public int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}

		public void Insert(int index, object value)
		{
			throw new NotImplementedException();
		}

		public bool IsFixedSize
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public object this[int index]
		{
			get { return _list[index]; }
			set { throw new NotImplementedException(); }
		}

		public void Remove(object value)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}
	}
}