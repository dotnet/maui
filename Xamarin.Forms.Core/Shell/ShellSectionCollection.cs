using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	public sealed class ShellSectionCollection :  IList<ShellSection>, INotifyCollectionChanged
	{
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)Inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)Inner).CollectionChanged -= value; }
		}

		public int Count => Inner.Count;
		public bool IsReadOnly => Inner.IsReadOnly;
		internal IList<ShellSection> Inner { get; set; }

		public ShellSection this[int index]
		{
			get => Inner[index];
			set => Inner[index] = value;
		}

		public void Add(ShellSection item) => Inner.Add(item);

		public void Clear() => Inner.Clear();

		public bool Contains(ShellSection item) => Inner.Contains(item);

		public void CopyTo(ShellSection[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public IEnumerator<ShellSection> GetEnumerator() => Inner.GetEnumerator();

		public int IndexOf(ShellSection item) => Inner.IndexOf(item);

		public void Insert(int index, ShellSection item) => Inner.Insert(index, item);

		public bool Remove(ShellSection item) => Inner.Remove(item);

		public void RemoveAt(int index) => Inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Inner).GetEnumerator();
	}
}