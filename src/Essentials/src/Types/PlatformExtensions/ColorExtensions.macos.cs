using System;
using System.Drawing;
using AppKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static Color ToSystemColor(this NSColor color)
		{
			if (color == null)
				throw new ArgumentNullException(nameof(color));

			// make sure the colorspace is valid for RGBA
			// we can't check as the check will throw if it is invalid
			color = color.UsingColorSpace(NSColorSpace.SRGBColorSpace);

			color.GetRgba(out var red, out var green, out var blue, out var alpha);
			return Color.FromArgb((int)(alpha * 255), (int)(red * 255), (int)(green * 255), (int)(blue * 255));
		}

		public static NSColor ToPlatformColor(this Color color) =>
			NSColor.FromRgba(color.R, color.G, color.B, color.A);
	}
}
