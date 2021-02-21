using System;

namespace Xamarin.Forms
{
	public class SelectedItemChangedEventArgs : EventArgs
	{
		public SelectedItemChangedEventArgs(object selectedItem, int selectedItemIndex)
		{
			SelectedItem = selectedItem;
			SelectedItemIndex = selectedItemIndex;
		}

		public object SelectedItem { get; private set; }

		public int SelectedItemIndex { get; private set; }

	}
}