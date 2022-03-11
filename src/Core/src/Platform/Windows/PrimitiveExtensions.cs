using Microsoft.Maui.Graphics;
using WPoint = Windows.Foundation.Point;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	public static class PrimitiveExtensions
	{
		public static WPoint ToPlatform(this Point point) =>
			new WPoint(point.X, point.Y);

		public static WThickness ToPlatform(this Thickness thickness) =>
			new WThickness(
				thickness.Left,
				thickness.Top,
				thickness.Right,
				thickness.Bottom);
	}
}