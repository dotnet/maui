using System;

namespace Xamarin.Forms
{
	public sealed class ItemVisibilityEventArgs : EventArgs
	{
		public ItemVisibilityEventArgs(object item)
		{
			Item = item;
		}

		public object Item { get; private set; }
	}
}