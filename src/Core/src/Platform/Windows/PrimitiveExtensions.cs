using Microsoft.Maui.Graphics;
using WPoint = Windows.Foundation.Point;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui
{
	public static class PrimitiveExtensions
	{
		public static WPoint ToNative(this Point point) =>
			new WPoint(point.X, point.Y);

		public static WThickness ToNative(this Thickness thickness) =>
			new WThickness(
				thickness.Left,
				thickness.Top,
				thickness.Right,
				thickness.Bottom);
	}
}