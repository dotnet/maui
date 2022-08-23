using Microsoft.Maui.Graphics;
using EColor = ElmSharp.Color;
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

		public static EColor ToPlatformEFL(this Color c)
		{
			return c == null ? EColor.Default : new EColor((int)(255.0 * c.Red), (int)(255.0 * c.Green), (int)(255.0 * c.Blue), (int)(255.0 * c.Alpha));
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
	}
}