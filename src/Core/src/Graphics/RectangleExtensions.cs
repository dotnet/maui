#nullable enable
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Graphics
{
	internal static class RectangleExtensions
	{
		public static Rect Inset(this Rect rectangle, double inset)
		{
			if (inset == 0)
			{
				return rectangle;
			}

			return new Rect(rectangle.Left + inset, rectangle.Top + inset,
				rectangle.Width - (2 * inset), rectangle.Height - (2 * inset));
		}

		public static bool Contains(this Rect rect, Point point) =>
			point.X >= 0 && point.X <= rect.Width &&
			point.Y >= 0 && point.Y <= rect.Height;

		public static bool ContainsAny(this Rect rect, Point[] points)
			=> points.Any(x => rect.Contains(x));
	}
}