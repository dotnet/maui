using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Maui.Core
{
	public static class RectExtensions
	{
		public static Point Center(this Rectangle rectangle) => new Point(rectangle.X + (rectangle.Width / 2), rectangle.Y + (rectangle.Height / 2));
		public static void Center(this ref Rectangle rectangle, Point center)
		{
			var halfWidth = rectangle.Width / 2;
			var halfHeight = rectangle.Height / 2;
			rectangle.X = center.X - halfWidth;
			rectangle.Y = center.Y - halfHeight;
		}
		public static bool BoundsContains(this Rectangle rect, Point point) =>
			point.X >= 0 && point.X <= rect.Width &&
			point.Y >= 0 && point.Y <= rect.Height;

		public static bool Contains(this Rectangle rect, Point[] points)
			=> points.Any(x => rect.Contains(x));

		//public static Rectangle ApplyPadding(this Rectangle rect, Thickness thickness)
		//{
		//	if (thickness == null)
		//		return rect;
		//	rect.X += thickness.Left;
		//	rect.Y += thickness.Top;
		//	rect.Width -= thickness.HorizontalThickness;
		//	rect.Height -= thickness.VerticalThickness;

		//	return rect;
		//}
	}
}
