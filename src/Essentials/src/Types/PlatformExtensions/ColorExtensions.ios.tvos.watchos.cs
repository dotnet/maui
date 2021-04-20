using System;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static Color ToMauiColor(this UIColor color)
		{
			if (color == null)
				throw new ArgumentNullException(nameof(color));

			color.GetRGBA(out var red, out var green, out var blue, out var alpha);
			return new Color((float)alpha, (float)red, (float)green, (float)blue);
		}

		public static UIColor ToPlatformColor(this Color color) =>
			new UIColor(color.Red, color.Green, color.Blue, color.Alpha);
	}
}
