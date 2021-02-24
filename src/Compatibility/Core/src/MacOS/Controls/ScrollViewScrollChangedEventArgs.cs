using System;
using PointF = CoreGraphics.CGPoint;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	internal class ScrollViewScrollChangedEventArgs : EventArgs
	{
		public PointF CurrentScrollPoint { get; set; }
	}
}