#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the Scrolled event in items views.
	/// </summary>
	/// <remarks>
	/// This event args class contains information about scroll position changes in <see cref="CollectionView"/> and related controls.
	/// It includes both delta (change) values and absolute offset values, as well as information about which items are currently visible.
	/// </remarks>
	public class ItemsViewScrolledEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the horizontal distance scrolled since the last scroll event.
		/// </summary>
		/// <value>The horizontal scroll delta in platform-specific units.</value>
		public double HorizontalDelta { get; set; }

		/// <summary>
		/// Gets or sets the vertical distance scrolled since the last scroll event.
		/// </summary>
		/// <value>The vertical scroll delta in platform-specific units.</value>
		public double VerticalDelta { get; set; }

		/// <summary>
		/// Gets or sets the current horizontal scroll position.
		/// </summary>
		/// <value>The horizontal scroll offset from the start position in platform-specific units.</value>
		public double HorizontalOffset { get; set; }

		/// <summary>
		/// Gets or sets the current vertical scroll position.
		/// </summary>
		/// <value>The vertical scroll offset from the start position in platform-specific units.</value>
		public double VerticalOffset { get; set; }

		/// <summary>
		/// Gets or sets the index of the first visible item in the view.
		/// </summary>
		/// <value>The zero-based index of the first partially or fully visible item.</value>
		public int FirstVisibleItemIndex { get; set; }

		/// <summary>
		/// Gets or sets the index of the item at the center of the view.
		/// </summary>
		/// <value>The zero-based index of the item closest to the center of the visible area.</value>
		public int CenterItemIndex { get; set; }

		/// <summary>
		/// Gets or sets the index of the last visible item in the view.
		/// </summary>
		/// <value>The zero-based index of the last partially or fully visible item.</value>
		public int LastVisibleItemIndex { get; set; }
	}
}