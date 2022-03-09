using System.Globalization;

namespace Microsoft.Maui.Graphics
{
	public partial class Color
	{
		private static Color FromArgbImplementation(string colorAsHex)
		{
			//Skip # if present
			if (colorAsHex[0] == '#')
				colorAsHex = colorAsHex.Substring(1);

			int red = 0;
			int green = 0;
			int blue = 0;
			int alpha = 255;

			if (colorAsHex.Length == 6)
			{
				//#RRGGBB
				red = ParseInt(colorAsHex.Substring(0, 2));
				green = ParseInt(colorAsHex.Substring(2, 2));
				blue = ParseInt(colorAsHex.Substring(4, 2));
			}
			else if (colorAsHex.Length == 3)
			{
				//#RGB
				red = ParseInt($"{colorAsHex[0]}{colorAsHex[0]}");
				green = ParseInt($"{colorAsHex[1]}{colorAsHex[1]}");
				blue = ParseInt($"{colorAsHex[2]}{colorAsHex[2]}");
			}
			else if (colorAsHex.Length == 4)
			{
				//#ARGB
				alpha = ParseInt($"{colorAsHex[0]}{colorAsHex[0]}");
				red = ParseInt($"{colorAsHex[1]}{colorAsHex[1]}");
				green = ParseInt($"{colorAsHex[2]}{colorAsHex[2]}");
				blue = ParseInt($"{colorAsHex[3]}{colorAsHex[3]}");
			}
			else if (colorAsHex.Length == 8)
			{
				//#AARRGGBB
				alpha = ParseInt(colorAsHex.Substring(0, 2));
				red = ParseInt(colorAsHex.Substring(2, 2));
				green = ParseInt(colorAsHex.Substring(4, 2));
				blue = ParseInt(colorAsHex.Substring(6, 2));
			}

			return FromRgba(red / 255f, green / 255f, blue / 255f, alpha / 255f);
		}

		private static int ParseInt(string s) =>
			int.Parse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

	}
}
