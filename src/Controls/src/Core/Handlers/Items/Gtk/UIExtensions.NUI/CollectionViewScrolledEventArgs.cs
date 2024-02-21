using System;

namespace Gtk.UIExtensions.NUI
{
    /// <summary>
    /// EventArgs for CollectionView Scrolled event
    /// </summary>
	public class CollectionViewScrolledEventArgs : EventArgs
	{
        /// <summary>
        /// Delta of horizontal just before event
        /// </summary>
		public double HorizontalDelta { get; set; }

        /// <summary>
        /// Delta of vertical just before event
        /// </summary>
		public double VerticalDelta { get; set; }

        /// <summary>
        /// Scrolled offset horizontally
        /// </summary>
		public double HorizontalOffset { get; set; }

        /// <summary>
        /// Scrolled offset vertically
        /// </summary>
		public double VerticalOffset { get; set; }

        /// <summary>
        /// First visible item on scrolled area
        /// </summary>
		public int FirstVisibleItemIndex { get; set; }

        /// <summary>
        /// Center item on scrolled area
        /// </summary>
		public int CenterItemIndex { get; set; }

        /// <summary>
        /// Last visible item on scrolled area
        /// </summary>
		public int LastVisibleItemIndex { get; set; }
	}
}