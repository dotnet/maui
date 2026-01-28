#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event args when an item's visibility has been changed in a <see cref="Microsoft.Maui.Controls.ListView"/>.</summary>
	public sealed class ItemVisibilityEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="ItemVisibilityEventArgs"/> with the specified item and index.</summary>
		/// <param name="item">The item whose visibility changed.</param>
		/// <param name="itemIndex">The index of the item.</param>
		public ItemVisibilityEventArgs(object item, int itemIndex)
		{
			Item = item;
			ItemIndex = itemIndex;
		}

		/// <summary>The item from the <see cref="Microsoft.Maui.Controls.ItemsView{T}.ItemsSource"/> whose visibility has changed.</summary>
		public object Item { get; private set; }

		/// <summary>Gets the index of the item within the items source.</summary>
		public int ItemIndex { get; private set; }
	}
}