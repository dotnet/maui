#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="Microsoft.Maui.Controls.ListView.ItemTapped"/> event.</summary>
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

		/// <summary>The collection of elements to which the tapped item belongs.</summary>
		public object Group { get; private set; }

		/// <summary>The visual element that the user tapped.</summary>
		public object Item { get; private set; }

		/// <summary>Gets the index of the tapped item within the items source.</summary>
		public int ItemIndex { get; private set; }
	}
}