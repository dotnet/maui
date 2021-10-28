using System;
using ElmSharp;

namespace Microsoft.Maui
{
	internal static class GeometryExtensions
	{
		public static Rect ExpandTo(this Rect geometry, IShadow? shadow)
		{
			double left = 0;
			double top = 0;
			double right = 0;
			double bottom = 0;

			var scaledOffsetX = shadow == null ? 0 : shadow.Offset.X.ToScaledPixel();
			var scaledOffsetY = shadow == null ? 0 : shadow.Offset.Y.ToScaledPixel();
			var scaledBlurRadius = shadow == null ? 0 : ((double)shadow.Radius).ToScaledPixel();
			var spreadSize = scaledBlurRadius * 3;
			var spreadLeft = scaledOffsetX - spreadSize;
			var spreadRight = scaledOffsetX + spreadSize;
			var spreadTop = scaledOffsetY - spreadSize;
			var spreadBottom = scaledOffsetY + spreadSize;
			if (left > spreadLeft)
				left = spreadLeft;
			if (top > spreadTop)
				top = spreadTop;
			if (right < spreadRight)
				right = spreadRight;
			if (bottom < spreadBottom)
				bottom = spreadBottom;

			var canvasGeometry = new Rect(
			geometry.X + (int)left,
			geometry.Y + (int)top,
			geometry.Width + (int)right - (int)left,
			geometry.Height + (int)bottom - (int)top);

			return canvasGeometry;
		}
	}
}