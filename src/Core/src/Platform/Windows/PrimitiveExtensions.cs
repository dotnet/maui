using Microsoft.Maui.Graphics;
using WPoint = Windows.Foundation.Point;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	public static class PrimitiveExtensions
	{
		/// <summary>
		/// Creates a <see cref="Point"/> from a <see cref="WPoint"/>.
		/// </summary>
		/// <param name="point">The point whose data will be used.</param>
		/// <returns>A point object compatible with the rest of .NET MAUI.</returns>
		public static Point ToPoint(this WPoint point) =>
			new Point(point.X, point.Y);

		public static WPoint ToPlatform(this Point point) =>
			new WPoint(point.X, point.Y);

		/// <summary>
		/// Creates a <see cref="Thickness"/> from a <see cref="WThickness"/>.
		/// </summary>
		/// <param name="thickness">The thickness whose data will be used.</param>
		/// <returns>A thickness object compatible with the rest of .NET MAUI.</returns>
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