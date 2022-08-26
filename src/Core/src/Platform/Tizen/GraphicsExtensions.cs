using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	internal static class GraphicsExtensions
	{
		public static Rect ExpandTo(this Rect geometry, Thickness shadowMargin)
		{
			var canvasGeometry = new Rect(
			geometry.X - shadowMargin.Left,
			geometry.Y - shadowMargin.Top,
			geometry.Width + shadowMargin.HorizontalThickness,
			geometry.Height + shadowMargin.VerticalThickness);

			return canvasGeometry;
		}

		public static Thickness GetShadowMargin(this IShadow shadow)
		{
			double left = 0;
			double top = 0;
			double right = 0;
			double bottom = 0;

			var offsetX = shadow == null ? 0 : shadow.Offset.X;
			var offsetY = shadow == null ? 0 : shadow.Offset.Y;
			var blurRadius = shadow == null ? 0 : ((double)shadow.Radius);
			var spreadSize = blurRadius * 3;
			var spreadLeft = offsetX - spreadSize;
			var spreadRight = offsetX + spreadSize;
			var spreadTop = offsetY - spreadSize;
			var spreadBottom = offsetY + spreadSize;
			if (left > spreadLeft)
				left = spreadLeft;
			if (top > spreadTop)
				top = spreadTop;
			if (right < spreadRight)
				right = spreadRight;
			if (bottom < spreadBottom)
				bottom = spreadBottom;

			return new Thickness(Math.Abs(left), Math.Abs(top), Math.Abs(right), Math.Abs(bottom));
		}
	}
}