#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="Microsoft.Maui.Controls.ListView.ItemTapped"/> and <see cref="Microsoft.Maui.Controls.SelectableItemsView.ItemTapped"/> events.</summary>
	public class ItemTappedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="ItemTappedEventArgs"/> with the specified group, item, and index.</summary>
		/// <param name="group">The group containing the tapped item.</param>
		/// <param name="item">The tapped item.</param>
		/// <param name="itemIndex">The index of the tapped item.</param>
		public ItemTappedEventArgs(object group, object item, int itemIndex)
		{
			Group = group;
			Item = item;
			ItemIndex = itemIndex;
		}

		/// <summary>The group to which the tapped item belongs, or <see langword="null"/> when the items are not grouped.</summary>
		public object Group { get; private set; }

		/// <summary>The data item that the user tapped.</summary>
		public object Item { get; private set; }

		/// <summary>Gets the index of the tapped item within the items source. When the items are grouped, this is the position in the flattened list in which each group occupies one row for its header followed by its items.</summary>
		public int ItemIndex { get; private set; }
	}
}