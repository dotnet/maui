using System.Linq;
using Microsoft.Maui;

namespace Microsoft.Maui
{
	internal static class RectExtensions
	{
		public static bool Contains(this Rectangle rect, Point point) =>
			point.X >= 0 && point.X <= rect.Width &&
			point.Y >= 0 && point.Y <= rect.Height;

		public static bool ContainsAny(this Rectangle rect, Point[] points)
			=> points.Any(x => rect.Contains(x));
	}
}
