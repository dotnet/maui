using Microsoft.Maui.Graphics;
using WindowsColor = Windows.UI.Color;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static Color ToMauiColor(this WindowsColor color) =>
			Color.FromRgba(color.R, color.G, color.B, color.A);

		public static WindowsColor ToPlatformColor(this Color color)
		{
			color.ToRgba(out var r, out var g, out var b, out var a);
			return WindowsColor.FromArgb(a, r, g, b);
		}
	}
}
