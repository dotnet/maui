using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Maui
{
	public sealed class MenuItemCollection : IEnumerable<MenuItem>, IList<MenuItem>, INotifyCollectionChanged
	{
		ObservableCollection<MenuItem> _inner = new ObservableCollection<MenuItem>();

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)_inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)_inner).CollectionChanged -= value; }
		}

		public int Count => _inner.Count;

		public bool IsReadOnly => ((IList<MenuItem>)_inner).IsReadOnly;

		public MenuItem this[int index]
		{
			get => _inner[index];
			set => _inner[index] = value;
		}

		public void Add(MenuItem item) => _inner.Add(item);

		public void Clear() => _inner.Clear();

		public bool Contains(MenuItem item) => _inner.Contains(item);

		public void CopyTo(MenuItem[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

		public IEnumerator<MenuItem> GetEnumerator() => _inner.GetEnumerator();

		public int IndexOf(MenuItem item) => _inner.IndexOf(item);

		public void Insert(int index, MenuItem item) => _inner.Insert(index, item);

		public bool Remove(MenuItem item) => _inner.Remove(item);

		public void RemoveAt(int index) => _inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
	}
}