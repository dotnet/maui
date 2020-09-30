using System;
using PointF = CoreGraphics.CGPoint;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class ScrollViewScrollChangedEventArgs : EventArgs
	{
		public PointF CurrentScrollPoint { get; set; }
	}
}