using System;
namespace System.Maui
{
	public class CurrentItemChangedEventArgs : EventArgs
	{
		public object PreviousItem { get; }
		public object CurrentItem { get; }

		internal CurrentItemChangedEventArgs(object previousItem, object currentItem)
		{
			PreviousItem = previousItem;
			CurrentItem = currentItem;
		}
	}
}
