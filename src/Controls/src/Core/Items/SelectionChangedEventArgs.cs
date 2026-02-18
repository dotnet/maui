#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the SelectionChanged event in selectable items views.
	/// </summary>
	/// <remarks>
	/// This event args class is used when the selection changes in a <see cref="CollectionView"/>, <see cref="CarouselView"/>, or other selectable views.
	/// It provides both the previous and current selections as read-only lists to support both single and multiple selection modes.
	/// </remarks>
	public class SelectionChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the list of items that were previously selected before the change.
		/// </summary>
		/// <value>A read-only list of the previously selected items. Empty if no items were previously selected.</value>
		public IReadOnlyList<object> PreviousSelection { get; }
		
		/// <summary>
		/// Gets the list of items that are now selected after the change.
		/// </summary>
		/// <value>A read-only list of the currently selected items. Empty if no items are currently selected.</value>
		public IReadOnlyList<object> CurrentSelection { get; }

		static readonly IReadOnlyList<object> s_empty = new List<object>(0);

		internal SelectionChangedEventArgs(object previousSelection, object currentSelection)
		{
			PreviousSelection = previousSelection != null ? new List<object>(1) { previousSelection } : s_empty;
			CurrentSelection = currentSelection != null ? new List<object>(1) { currentSelection } : s_empty;
		}

		internal SelectionChangedEventArgs(IList<object> previousSelection, IList<object> currentSelection)
		{
			PreviousSelection = new List<object>(previousSelection ?? throw new ArgumentNullException(nameof(previousSelection)));
			CurrentSelection = new List<object>(currentSelection ?? throw new ArgumentNullException(nameof(currentSelection)));
		}
	}
}