using System;

namespace Xamarin.Forms
{
	public class ItemTappedEventArgs : EventArgs
	{
		public ItemTappedEventArgs(object group, object item)
		{
			Group = group;
			Item = item;
		}

		public object Group { get; private set; }

		public object Item { get; private set; }
	}
}