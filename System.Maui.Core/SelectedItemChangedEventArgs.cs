using System;

namespace System.Maui
{
	public class SelectedItemChangedEventArgs : EventArgs
	{
		[Obsolete("This constructor is obsolete as of version 3.5. Please use SelectedItemChangedEventArgs(object selectedItem, int selectedItemIndex) instead.")]
		public SelectedItemChangedEventArgs(object selectedItem)
			: this(selectedItem, -1)
		{

		}

		public SelectedItemChangedEventArgs(object selectedItem, int selectedItemIndex)
		{
			SelectedItem = selectedItem;
			SelectedItemIndex = selectedItemIndex;
		}

		public object SelectedItem { get; private set; }

		public int SelectedItemIndex { get; private set; }

	}
}