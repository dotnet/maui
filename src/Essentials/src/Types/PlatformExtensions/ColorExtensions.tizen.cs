using Microsoft.Maui.Graphics;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static Color ToMauiColor(this EColor color) =>
			Color.FromRgba(color.A, color.R, color.G, color.B);

		public static EColor ToPlatformColor(this Color color)
		{
			color.ToRgba(out var r, out var g, out var b, out var a);
			return new EColor(r, g, b, a);
		}
	}
}
