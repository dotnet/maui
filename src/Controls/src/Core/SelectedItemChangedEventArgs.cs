#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="ListView.ItemSelected"/> event.</summary>
	public class SelectedItemChangedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="SelectedItemChangedEventArgs"/> with the specified item and index.</summary>
		/// <param name="selectedItem">The newly selected item.</param>
		/// <param name="selectedItemIndex">The index of the selected item in the items source.</param>
		public SelectedItemChangedEventArgs(object selectedItem, int selectedItemIndex)
		{
			SelectedItem = selectedItem;
			SelectedItemIndex = selectedItemIndex;
		}

		/// <summary>Gets the newly selected item, or <see langword="null"/> if an item was deselected.</summary>
		public object SelectedItem { get; private set; }

		/// <summary>Gets the index of the selected item, or -1 if an item was deselected.</summary>
		public int SelectedItemIndex { get; private set; }

	}
}