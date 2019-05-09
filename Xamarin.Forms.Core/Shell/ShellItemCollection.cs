using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	internal sealed class ShellItemCollection : IList<ShellItem>, INotifyCollectionChanged
	{
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)Inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)Inner).CollectionChanged -= value; }
		}

		public int Count => Inner.Count;
		public bool IsReadOnly => ((IList<ShellItem>)Inner).IsReadOnly;
		internal IList<ShellItem> Inner { get; set; }

		public ShellItem this[int index]
		{
			get => Inner[index];
			set => Inner[index] = value;
		}

		public void Add(ShellItem item)
		{
			/*
			 * This is purely for the case where a user is only specifying Tabs at the highest level
			 * <shell>
			 * <tab></tab>
			 * <tab></tab>
			 * </shell>
			 * */
			if (Routing.IsImplicit(item) &&
				item is TabBar
				)
			{
				int i = Count - 1;
				if (i >= 0 &&  this[i] is TabBar && Routing.IsImplicit(this[i]))
				{
					this[i].Items.Add(item.Items[0]);
					return;
				}
			}

			Inner.Add(item);
		}

		public void Clear() => Inner.Clear();

		public bool Contains(ShellItem item) => Inner.Contains(item);

		public void CopyTo(ShellItem[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public IEnumerator<ShellItem> GetEnumerator() => Inner.GetEnumerator();

		public int IndexOf(ShellItem item) => Inner.IndexOf(item);

		public void Insert(int index, ShellItem item) => Inner.Insert(index, item);

		public bool Remove(ShellItem item) => Inner.Remove(item);

		public void RemoveAt(int index) => Inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Inner).GetEnumerator();
	}
}
