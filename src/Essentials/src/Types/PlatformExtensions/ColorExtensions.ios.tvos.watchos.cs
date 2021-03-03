using System;
using System.Drawing;
using UIKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static Color ToSystemColor(this UIColor color)
		{
			if (color == null)
				throw new ArgumentNullException(nameof(color));

			color.GetRGBA(out var red, out var green, out var blue, out var alpha);
			return Color.FromArgb((int)(alpha * 255), (int)(red * 255), (int)(green * 255), (int)(blue * 255));
		}

		public static UIColor ToPlatformColor(this Color color) =>
			new UIColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
	}
}
