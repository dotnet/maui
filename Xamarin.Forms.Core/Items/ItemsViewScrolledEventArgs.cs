using System;

namespace Xamarin.Forms
{
	public class ItemsViewScrolledEventArgs : EventArgs
	{
		public double HorizontalDelta { get; set; }

		public double VerticalDelta { get; set; }

		public double HorizontalOffset { get; set; }

		public double VerticalOffset { get; set; }

		public int FirstVisibleItemIndex { get; set; }

		public int CenterItemIndex { get; set; }

		public int LastVisibleItemIndex { get; set; }
	}
}