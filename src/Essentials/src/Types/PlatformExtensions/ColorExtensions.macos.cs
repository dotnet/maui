using System;
using Microsoft.Maui.Graphics;
using AppKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static Color ToMauiColor(this NSColor color)
		{
			if (color == null)
				throw new ArgumentNullException(nameof(color));

			// make sure the colorspace is valid for RGBA
			// we can't check as the check will throw if it is invalid
			color = color.UsingColorSpace(NSColorSpace.SRGBColorSpace);

			color.GetRgba(out var red, out var green, out var blue, out var alpha);
			return new Color((float)alpha, (float)red, (float)green, (float)blue);
		}

		public static NSColor ToPlatformColor(this Color color) =>
			NSColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);
	}
}
