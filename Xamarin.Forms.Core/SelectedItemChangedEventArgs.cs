using System;

namespace Xamarin.Forms
{
	public class SelectedItemChangedEventArgs : EventArgs
	{
		public SelectedItemChangedEventArgs(object selectedItem)
		{
			SelectedItem = selectedItem;
		}

		public object SelectedItem { get; private set; }
	}
}