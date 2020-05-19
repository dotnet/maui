using System;
using PointF = CoreGraphics.CGPoint;

namespace System.Maui.Platform.MacOS
{
	internal class ScrollViewScrollChangedEventArgs : EventArgs
	{
		public PointF CurrentScrollPoint { get; set; }
	}
}