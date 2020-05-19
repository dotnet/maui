using System;

namespace System.Maui
{
	public class ItemTappedEventArgs : EventArgs
	{
		[Obsolete("Please use the constructor that reports the items index")]
		public ItemTappedEventArgs(object group, object item)
			: this(group, item, -1)
		{

		}

		public ItemTappedEventArgs(object group, object item, int itemIndex)
		{
			Group = group;
			Item = item;
			ItemIndex = itemIndex;
		}

		public object Group { get; private set; }

		public object Item { get; private set; }

		public int ItemIndex { get; private set; }
	}
}