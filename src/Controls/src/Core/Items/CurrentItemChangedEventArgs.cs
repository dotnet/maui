#nullable disable
using System;
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the CurrentItemChanged event in carousel and collection views.
	/// </summary>
	/// <remarks>
	/// This event args class is used when the current item changes in a <see cref="CarouselView"/> or similar control.
	/// It provides both the previous and current items to allow tracking of item changes.
	/// </remarks>
	public class CurrentItemChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the item that was previously current before the change.
		/// </summary>
		/// <value>The previously current item, or <see langword="null"/> if there was no previous item.</value>
		public object PreviousItem { get; }
		
		/// <summary>
		/// Gets the item that is now current after the change.
		/// </summary>
		/// <value>The currently selected item, or <see langword="null"/> if no item is current.</value>
		public object CurrentItem { get; }

		internal CurrentItemChangedEventArgs(object previousItem, object currentItem)
		{
			PreviousItem = previousItem;
			CurrentItem = currentItem;
		}
	}
}
