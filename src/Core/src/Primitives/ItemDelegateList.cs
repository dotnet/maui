using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public class ItemDelegateList<T> : IReadOnlyList<T>
	{
		public ItemDelegateList(IItemDelegate<T> itemDelegate)
		{
			ItemDelegate = itemDelegate;
		}
		public T this[int index] => ItemDelegate.GetItem(index);

		public int Count => ItemDelegate.GetCount();

		public IItemDelegate<T> ItemDelegate { get; }

		public IEnumerator<T> GetEnumerator()
		{
			var count = ItemDelegate.GetCount();
			for (var i = 0; i < count; i++)
				yield return ItemDelegate.GetItem(i);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			var count = ItemDelegate.GetCount();
			for (var i = 0; i < count; i++)
				yield return ItemDelegate.GetItem(i);
		}
	}
}
