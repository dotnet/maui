using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	internal sealed class ShellContentCollection : IList<ShellContent>, INotifyCollectionChanged
	{
		ObservableCollection<ShellContent> _inner = new ObservableCollection<ShellContent>();

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)_inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)_inner).CollectionChanged -= value; }
		}

		public int Count => _inner.Count;

		public bool IsReadOnly => ((IList<ShellContent>)_inner).IsReadOnly;

		public ShellContent this[int index]
		{
			get => _inner[index];
			set => _inner[index] = value;
		}

		public void Add(ShellContent item) => _inner.Add(item);

		public void Clear() => _inner.Clear();

		public bool Contains(ShellContent item) => _inner.Contains(item);

		public void CopyTo(ShellContent[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

		public IEnumerator<ShellContent> GetEnumerator() => _inner.GetEnumerator();

		public int IndexOf(ShellContent item) => _inner.IndexOf(item);

		public void Insert(int index, ShellContent item) => _inner.Insert(index, item);

		public bool Remove(ShellContent item) => _inner.Remove(item);

		public void RemoveAt(int index) => _inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
	}
}