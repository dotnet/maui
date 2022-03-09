using System;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
	public partial class Color
	{
		private static Color FromArgbImplementation(string colorAsHex)
		{
			ReadOnlySpan<char> colorAsHexSpan = colorAsHex;

			//Skip # if present
			if (colorAsHexSpan[0] == '#')
				colorAsHexSpan = colorAsHexSpan.Slice(1);

			int red = 0;
			int green = 0;
			int blue = 0;
			int alpha = 255;

			if (colorAsHexSpan.Length == 6)
			{
				//#RRGGBB
				red = ParseInt(colorAsHexSpan.Slice(0, 2));
				green = ParseInt(colorAsHexSpan.Slice(2, 2));
				blue = ParseInt(colorAsHexSpan.Slice(4, 2));
			}
			else if (colorAsHexSpan.Length == 3)
			{
				//#RGB
				Span<char> temp = stackalloc char[2];
				temp[0] = temp[1] = colorAsHexSpan[0];
				red = ParseInt(temp);

				temp[0] = temp[1] = colorAsHexSpan[1];
				green = ParseInt(temp);

				temp[0] = temp[1] = colorAsHexSpan[2];
				blue = ParseInt(temp);
			}
			else if (colorAsHexSpan.Length == 4)
			{
				//#ARGB
				Span<char> temp = stackalloc char[2];
				temp[0] = temp[1] = colorAsHexSpan[0];
				alpha = ParseInt(temp);

				temp[0] = temp[1] = colorAsHexSpan[1];
				red = ParseInt(temp);

				temp[0] = temp[1] = colorAsHexSpan[2];
				green = ParseInt(temp);

				temp[0] = temp[1] = colorAsHexSpan[3];
				blue = ParseInt(temp);
			}
			else if (colorAsHexSpan.Length == 8)
			{
				//#AARRGGBB
				alpha = ParseInt(colorAsHexSpan.Slice(0, 2));
				red = ParseInt(colorAsHexSpan.Slice(2, 2));
				green = ParseInt(colorAsHexSpan.Slice(4, 2));
				blue = ParseInt(colorAsHexSpan.Slice(6, 2));
			}

			return FromRgba(red / 255f, green / 255f, blue / 255f, alpha / 255f);
		}

		private static int ParseInt(ReadOnlySpan<char> s) =>
			int.Parse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
	}
}
