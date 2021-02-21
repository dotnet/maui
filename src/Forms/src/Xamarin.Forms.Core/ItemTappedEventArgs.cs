using System;

namespace Xamarin.Forms
{
	public class ItemTappedEventArgs : EventArgs
	{
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