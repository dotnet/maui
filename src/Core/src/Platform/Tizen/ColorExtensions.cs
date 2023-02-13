using Microsoft.Maui.Graphics;
using NColor = Tizen.NUI.Color;
using TColor = Tizen.UIExtensions.Common.Color;
using TLog = Tizen.UIExtensions.Common.Log;

namespace Microsoft.Maui.Platform
{
	public static class ColorExtensions
	{
		public static TColor ToPlatform(this Color c)
		{
			return c == null ? TColor.Default : new TColor(c.Red, c.Green, c.Blue, c.Alpha);
		}

		public static NColor? ToNUIColor(this Color c)
		{
			return c == null ? null : new NColor(c.Red, c.Green, c.Blue, c.Alpha);
		}

		public static Color WithAlpha(this Color color, double alpha)
		{
			return new Color(color.Red, color.Green, color.Blue, (int)(255 * alpha));
		}

		public static Color WithPremultiplied(this Color color, double alpha)
		{
			return new Color((int)(color.Red * alpha), (int)(color.Green * alpha), (int)(color.Blue * alpha), color.Alpha);
		}

		internal static string ToHex(this TColor c)
		{
			if (c.IsDefault)
			{
				TLog.Warn("Trying to convert the default color to hexagonal notation, it does not works as expected.");
			}
			return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
		}

		public static Color GetAccentColor(this Color color)
		{
			var grayscale = (color.Red + color.Green + color.Blue) / 3.0f;
			return (grayscale > 0.6) || (color.Alpha < 0.5) ? Colors.Black : Colors.White;
		}
	}
}