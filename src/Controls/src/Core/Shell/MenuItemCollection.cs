#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls
{
	/// <summary>A collection of <see cref="MenuItem"/> objects used in Shell.</summary>
	public sealed class MenuItemCollection : IEnumerable<MenuItem>, IList<MenuItem>, INotifyCollectionChanged
	{
		ObservableCollection<MenuItem> _inner = new ObservableCollection<MenuItem>();

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)_inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)_inner).CollectionChanged -= value; }
		}

		/// <summary>Gets the number of menu items in the collection.</summary>
		public int Count => _inner.Count;

		/// <summary>Gets a value indicating whether the collection is read-only.</summary>
		public bool IsReadOnly => ((IList<MenuItem>)_inner).IsReadOnly;

		public MenuItem this[int index]
		{
			get => _inner[index];
			set => _inner[index] = value;
		}

		/// <summary>Adds a menu item to the collection.</summary>
		public void Add(MenuItem item) => _inner.Add(item);

		/// <summary>Removes all menu items from the collection.</summary>
		public void Clear() => _inner.Clear();

		/// <summary>Determines whether the collection contains a specific menu item.</summary>
		public bool Contains(MenuItem item) => _inner.Contains(item);

		/// <summary>Copies the menu items to an array, starting at the specified index.</summary>
		public void CopyTo(MenuItem[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		public IEnumerator<MenuItem> GetEnumerator() => _inner.GetEnumerator();

		/// <summary>Returns the index of the specified menu item.</summary>
		public int IndexOf(MenuItem item) => _inner.IndexOf(item);

		/// <summary>Inserts a menu item at the specified index.</summary>
		public void Insert(int index, MenuItem item) => _inner.Insert(index, item);

		/// <summary>Removes the specified menu item from the collection.</summary>
		public bool Remove(MenuItem item) => _inner.Remove(item);

		/// <summary>Removes the menu item at the specified index.</summary>
		public void RemoveAt(int index) => _inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
	}
}