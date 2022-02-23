using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	internal static class GeometryExtensions
	{
		public static Rectangle ExpandTo(this Rectangle geometry, IShadow? shadow)
		{
			double left = 0;
			double top = 0;
			double right = 0;
			double bottom = 0;

			var scaledOffsetX = shadow == null ? 0 : shadow.Offset.X;
			var scaledOffsetY = shadow == null ? 0 : shadow.Offset.Y;
			var scaledBlurRadius = shadow == null ? 0 : ((double)shadow.Radius);
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

			var canvasGeometry = new Rectangle(
			geometry.X + (int)left,
			geometry.Y + (int)top,
			geometry.Width + (int)right - (int)left,
			geometry.Height + (int)bottom - (int)top);

			return canvasGeometry;
		}
	}
}