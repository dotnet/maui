using System;

namespace Microsoft.Maui.Controls
{
	public sealed class ItemVisibilityEventArgs : EventArgs
	{
		public ItemVisibilityEventArgs(object item, int itemIndex)
		{
			Item = item;
			ItemIndex = itemIndex;
		}

		public object Item { get; private set; }

		public int ItemIndex { get; private set; }
	}
}