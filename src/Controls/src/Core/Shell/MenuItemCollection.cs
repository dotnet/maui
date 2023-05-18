#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="Type[@FullName='Microsoft.Maui.Controls.MenuItemCollection']/Docs/*" />
	public sealed class MenuItemCollection : IEnumerable<MenuItem>, IList<MenuItem>, INotifyCollectionChanged
	{
		ObservableCollection<MenuItem> _inner = new ObservableCollection<MenuItem>();

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)_inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)_inner).CollectionChanged -= value; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='Count']/Docs/*" />
		public int Count => _inner.Count;

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='IsReadOnly']/Docs/*" />
		public bool IsReadOnly => ((IList<MenuItem>)_inner).IsReadOnly;

		public MenuItem this[int index]
		{
			get => _inner[index];
			set => _inner[index] = value;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='Add']/Docs/*" />
		public void Add(MenuItem item) => _inner.Add(item);

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='Clear']/Docs/*" />
		public void Clear() => _inner.Clear();

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='Contains']/Docs/*" />
		public bool Contains(MenuItem item) => _inner.Contains(item);

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='CopyTo']/Docs/*" />
		public void CopyTo(MenuItem[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='GetEnumerator']/Docs/*" />
		public IEnumerator<MenuItem> GetEnumerator() => _inner.GetEnumerator();

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='IndexOf']/Docs/*" />
		public int IndexOf(MenuItem item) => _inner.IndexOf(item);

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='Insert']/Docs/*" />
		public void Insert(int index, MenuItem item) => _inner.Insert(index, item);

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='Remove']/Docs/*" />
		public bool Remove(MenuItem item) => _inner.Remove(item);

		/// <include file="../../../docs/Microsoft.Maui.Controls/MenuItemCollection.xml" path="//Member[@MemberName='RemoveAt']/Docs/*" />
		public void RemoveAt(int index) => _inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
	}
}