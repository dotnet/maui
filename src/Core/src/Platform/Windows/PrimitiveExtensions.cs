using Microsoft.Maui.Graphics;
using WPoint = Windows.Foundation.Point;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	public static class PrimitiveExtensions
	{
		public static Point ToPoint(this WPoint point) =>
			new Point(point.X, point.Y);

		public static WPoint ToPlatform(this Point point) =>
			new WPoint(point.X, point.Y);

		public static Thickness ToThickness(this WThickness thickness) =>
			new Thickness(
				thickness.Left,
				thickness.Top,
				thickness.Right,
				thickness.Bottom);

		public static WThickness ToPlatform(this Thickness thickness) =>
			new WThickness(
				thickness.Left,
				thickness.Top,
				thickness.Right,
				thickness.Bottom);
	}
}