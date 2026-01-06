using System;
using System.Globalization;

namespace Microsoft.Maui.Graphics;

internal static class ColorUtils
{
	public static bool TryParse(ReadOnlySpan<char> value, out float red, out float green, out float blue, out float alpha)
	{
		red = green = blue = alpha = 0f;
		
		value = value.Trim();
		if (value.IsEmpty)
			return false;

		if (value[0] == '#')
		{
			try
			{
				(red, green, blue, alpha) = FromArgb(value);
				return true;
			}
			catch
			{
				return false;
			}
		}

		if (value.StartsWith("rgba".AsSpan(), StringComparison.OrdinalIgnoreCase))
		{
			if (!TryParseFourColorRanges(value,
				out ReadOnlySpan<char> quad0,
				out ReadOnlySpan<char> quad1,
				out ReadOnlySpan<char> quad2,
				out ReadOnlySpan<char> quad3))
			{
				return false;
			}

			bool valid = TryParseColorValue(quad0, 255, acceptPercent: true, out double r);
			valid &= TryParseColorValue(quad1, 255, acceptPercent: true, out double g);
			valid &= TryParseColorValue(quad2, 255, acceptPercent: true, out double b);
			valid &= TryParseOpacity(quad3, out double a);

			if (!valid)
				return false;

			red = (float)r;
			green = (float)g;
			blue = (float)b;
			alpha = (float)a;
			return true;
		}

		if (value.StartsWith("rgb".AsSpan(), StringComparison.OrdinalIgnoreCase))
		{
			if (!TryParseThreeColorRanges(value,
				out ReadOnlySpan<char> triplet0,
				out ReadOnlySpan<char> triplet1,
				out ReadOnlySpan<char> triplet2))
			{
				return false;
			}

			bool valid = TryParseColorValue(triplet0, 255, acceptPercent: true, out double r);
			valid &= TryParseColorValue(triplet1, 255, acceptPercent: true, out double g);
			valid &= TryParseColorValue(triplet2, 255, acceptPercent: true, out double b);

			if (!valid)
				return false;

			red = (float)r;
			green = (float)g;
			blue = (float)b;
			alpha = 1.0f;
			return true;
		}

		if (value.StartsWith("hsla".AsSpan(), StringComparison.OrdinalIgnoreCase))
		{
			if (!TryParseFourColorRanges(value,
				out ReadOnlySpan<char> quad0,
				out ReadOnlySpan<char> quad1,
				out ReadOnlySpan<char> quad2,
				out ReadOnlySpan<char> quad3))
			{
				return false;
			}

			bool valid = TryParseColorValue(quad0, 360, acceptPercent: false, out double h);
			valid &= TryParseColorValue(quad1, 100, acceptPercent: true, out double s);
			valid &= TryParseColorValue(quad2, 100, acceptPercent: true, out double l);
			valid &= TryParseOpacity(quad3, out double a);

			if (!valid)
				return false;

			(red, green, blue) = ConvertHslToRgb((float)h, (float)s, (float)l);
			alpha = (float)a;
			return true;
		}

		if (value.StartsWith("hsl".AsSpan(), StringComparison.OrdinalIgnoreCase))
		{
			if (!TryParseThreeColorRanges(value,
				out ReadOnlySpan<char> triplet0,
				out ReadOnlySpan<char> triplet1,
				out ReadOnlySpan<char> triplet2))
			{
				return false;
			}

			bool valid = TryParseColorValue(triplet0, 360, acceptPercent: false, out double h);
			valid &= TryParseColorValue(triplet1, 100, acceptPercent: true, out double s);
			valid &= TryParseColorValue(triplet2, 100, acceptPercent: true, out double l);

			if (!valid)
				return false;

			(red, green, blue) = ConvertHslToRgb((float)h, (float)s, (float)l);
			alpha = 1.0f;
			return true;
		}

		if (value.StartsWith("hsva".AsSpan(), StringComparison.OrdinalIgnoreCase))
		{
			if (!TryParseFourColorRanges(value,
				out ReadOnlySpan<char> quad0,
				out ReadOnlySpan<char> quad1,
				out ReadOnlySpan<char> quad2,
				out ReadOnlySpan<char> quad3))
			{
				return false;
			}

			bool valid = TryParseColorValue(quad0, 360, acceptPercent: false, out double h);
			valid &= TryParseColorValue(quad1, 100, acceptPercent: true, out double s);
			valid &= TryParseColorValue(quad2, 100, acceptPercent: true, out double v);
			valid &= TryParseOpacity(quad3, out double a);

			if (!valid)
				return false;

			(red, green, blue) = ConvertHsvToRgb((float)h, (float)s, (float)v);
			alpha = (float)a;
			return true;
		}

		if (value.StartsWith("hsv".AsSpan(), StringComparison.OrdinalIgnoreCase))
		{
			if (!TryParseThreeColorRanges(value,
				out ReadOnlySpan<char> triplet0,
				out ReadOnlySpan<char> triplet1,
				out ReadOnlySpan<char> triplet2))
			{
				return false;
			}

			bool valid = TryParseColorValue(triplet0, 360, acceptPercent: false, out double h);
			valid &= TryParseColorValue(triplet1, 100, acceptPercent: true, out double s);
			valid &= TryParseColorValue(triplet2, 100, acceptPercent: true, out double v);

			if (!valid)
				return false;

			(red, green, blue) = ConvertHsvToRgb((float)h, (float)s, (float)v);
			alpha = 1.0f;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Converts HSL values to RGB.
	/// </summary>
	/// <param name="hue">Hue (0.0-1.0)</param>
	/// <param name="saturation">Saturation (0.0-1.0)</param>
	/// <param name="luminosity">Luminosity (0.0-1.0)</param>
	/// <returns>RGB components as (red, green, blue) where each component is 0.0-1.0</returns>
	public static (float red, float green, float blue) ConvertHslToRgb(float hue, float saturation, float luminosity)
	{
		if (luminosity == 0)
		{
			return (0, 0, 0);
		}

		if (saturation == 0)
		{
			return (luminosity, luminosity, luminosity);
		}

		float temp2 = luminosity <= 0.5f ? luminosity * (1.0f + saturation) : luminosity + saturation - luminosity * saturation;
		float temp1 = 2.0f * luminosity - temp2;

		var t3 = new[] { hue + 1.0f / 3.0f, hue, hue - 1.0f / 3.0f };
		var clr = new float[] { 0, 0, 0 };
		for (var i = 0; i < 3; i++)
		{
			if (t3[i] < 0)
				t3[i] += 1.0f;
			if (t3[i] > 1)
				t3[i] -= 1.0f;
			if (6.0 * t3[i] < 1.0)
				clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0f;
			else if (2.0 * t3[i] < 1.0)
				clr[i] = temp2;
			else if (3.0 * t3[i] < 2.0)
				clr[i] = temp1 + (temp2 - temp1) * (2.0f / 3.0f - t3[i]) * 6.0f;
			else
				clr[i] = temp1;
		}

		return (clr[0], clr[1], clr[2]);
	}

	public static (float red, float green, float blue, float alpha) FromRgba(ReadOnlySpan<char> colorAsHex)
	{
		int red = 0;
		int green = 0;
		int blue = 0;
		int alpha = 255;

		if (!colorAsHex.IsEmpty)
		{
			//Skip # if present
			if (colorAsHex[0] == '#')
				colorAsHex = colorAsHex.Slice(1);

			if (colorAsHex.Length == 6 || colorAsHex.Length == 3)
			{
				//#RRGGBB or #RGB - since there is no A, use FromArgb

				return FromArgb(colorAsHex);
			}
			else if (colorAsHex.Length == 4)
			{
				//#RGBA
				Span<char> temp = stackalloc char[2];
				temp[0] = temp[1] = colorAsHex[0];
				red = ParseInt(temp);

				temp[0] = temp[1] = colorAsHex[1];
				green = ParseInt(temp);

				temp[0] = temp[1] = colorAsHex[2];
				blue = ParseInt(temp);

				temp[0] = temp[1] = colorAsHex[3];
				alpha = ParseInt(temp);
			}
			else if (colorAsHex.Length == 8)
			{
				//#RRGGBBAA
				red = ParseInt(colorAsHex.Slice(0, 2));
				green = ParseInt(colorAsHex.Slice(2, 2));
				blue = ParseInt(colorAsHex.Slice(4, 2));
				alpha = ParseInt(colorAsHex.Slice(6, 2));
			}
		}

		return (red / 255f, green / 255f, blue / 255f, alpha / 255f);
	}

	public static (float red, float green, float blue, float alpha) FromArgb(ReadOnlySpan<char> colorAsHex)
	{
		int red = 0;
		int green = 0;
		int blue = 0;
		int alpha = 255;

		if (!colorAsHex.IsEmpty)
		{
			//Skip # if present
			if (colorAsHex[0] == '#')
				colorAsHex = colorAsHex.Slice(1);

			if (colorAsHex.Length == 6)
			{
				//#RRGGBB
				red = ParseInt(colorAsHex.Slice(0, 2));
				green = ParseInt(colorAsHex.Slice(2, 2));
				blue = ParseInt(colorAsHex.Slice(4, 2));
			}
			else if (colorAsHex.Length == 3)
			{
				//#RGB
				Span<char> temp = stackalloc char[2];
				temp[0] = temp[1] = colorAsHex[0];
				red = ParseInt(temp);

				temp[0] = temp[1] = colorAsHex[1];
				green = ParseInt(temp);

				temp[0] = temp[1] = colorAsHex[2];
				blue = ParseInt(temp);
			}
			else if (colorAsHex.Length == 4)
			{
				//#ARGB
				Span<char> temp = stackalloc char[2];
				temp[0] = temp[1] = colorAsHex[0];
				alpha = ParseInt(temp);

				temp[0] = temp[1] = colorAsHex[1];
				red = ParseInt(temp);

				temp[0] = temp[1] = colorAsHex[2];
				green = ParseInt(temp);

				temp[0] = temp[1] = colorAsHex[3];
				blue = ParseInt(temp);
			}
			else if (colorAsHex.Length == 8)
			{
				//#AARRGGBB
				alpha = ParseInt(colorAsHex.Slice(0, 2));
				red = ParseInt(colorAsHex.Slice(2, 2));
				green = ParseInt(colorAsHex.Slice(4, 2));
				blue = ParseInt(colorAsHex.Slice(6, 2));
			}
		}

		return (red / 255f, green / 255f, blue / 255f, alpha / 255f);
	}
	/// <summary>
	/// Converts HSV values to RGB.
	/// </summary>
	/// <param name="h">Hue (0.0-1.0)</param>
	/// <param name="s">Saturation (0.0-1.0)</param>
	/// <param name="v">Value (0.0-1.0)</param>
	/// <returns>RGB components as (red, green, blue) where each component is 0.0-1.0</returns>
	public static (float red, float green, float blue) ConvertHsvToRgb(float h, float s, float v)
	{
		h = h.Clamp(0, 1);
		s = s.Clamp(0, 1);
		v = v.Clamp(0, 1);
		
		var range = (int)(Math.Floor(h * 6)) % 6;
		var f = h * 6 - Math.Floor(h * 6);
		var p = v * (1 - s);
		var q = v * (1 - f * s);
		var t = v * (1 - (1 - f) * s);

		return range switch
		{
			0 => (v, (float)t, (float)p),
			1 => ((float)q, v, (float)p),
			2 => ((float)p, v, (float)t),
			3 => ((float)p, (float)q, v),
			4 => ((float)t, (float)p, v),
			_ => (v, (float)p, (float)q)
		};
	}

	private static bool TryParseFourColorRanges(
		ReadOnlySpan<char> value,
		out ReadOnlySpan<char> quad0,
		out ReadOnlySpan<char> quad1,
		out ReadOnlySpan<char> quad2,
		out ReadOnlySpan<char> quad3)
	{
		var op = value.IndexOf('(');
#pragma warning disable CA1307 // Specify StringComparison for clarity - char overload doesn't support StringComparison
		var cp = value.LastIndexOf(')');
#pragma warning restore CA1307 // Specify StringComparison for clarity
		if (op < 0 || cp < 0 || cp < op)
			goto ReturnFalse;

		value = value.Slice(op + 1, cp - op - 1);

		int index = value.IndexOf(',');
		if (index == -1)
			goto ReturnFalse;
		quad0 = value.Slice(0, index);
		value = value.Slice(index + 1);

		index = value.IndexOf(',');
		if (index == -1)
			goto ReturnFalse;
		quad1 = value.Slice(0, index);
		value = value.Slice(index + 1);

		index = value.IndexOf(',');
		if (index == -1)
			goto ReturnFalse;
		quad2 = value.Slice(0, index);
		quad3 = value.Slice(index + 1);

		// if there are more commas, fail
		if (quad3.IndexOf(',') != -1)
			goto ReturnFalse;

		return true;

	ReturnFalse:
		quad0 = quad1 = quad2 = quad3 = default;
		return false;
	}

	private static bool TryParseThreeColorRanges(
		ReadOnlySpan<char> value,
		out ReadOnlySpan<char> triplet0,
		out ReadOnlySpan<char> triplet1,
		out ReadOnlySpan<char> triplet2)
	{
		var op = value.IndexOf('(');
#pragma warning disable CA1307 // Specify StringComparison for clarity - char overload doesn't support StringComparison
		var cp = value.LastIndexOf(')');
#pragma warning restore CA1307 // Specify StringComparison for clarity
		if (op < 0 || cp < 0 || cp < op)
			goto ReturnFalse;

		value = value.Slice(op + 1, cp - op - 1);

		int index = value.IndexOf(',');
		if (index == -1)
			goto ReturnFalse;
		triplet0 = value.Slice(0, index);
		value = value.Slice(index + 1);

		index = value.IndexOf(',');
		if (index == -1)
			goto ReturnFalse;
		triplet1 = value.Slice(0, index);
		triplet2 = value.Slice(index + 1);

		// if there are more commas, fail
		if (triplet2.IndexOf(',') != -1)
			goto ReturnFalse;

		return true;

	ReturnFalse:
		triplet0 = triplet1 = triplet2 = default;
		return false;
	}

	private static bool TryParseColorValue(ReadOnlySpan<char> elem, int maxValue, bool acceptPercent, out double value)
	{
		elem = elem.Trim();
		if (!elem.IsEmpty && elem[elem.Length - 1] == '%' && acceptPercent)
		{
			maxValue = 100;
			elem = elem.Slice(0, elem.Length - 1);
		}

		if (TryParseDouble(elem, out value))
		{
			value = value.Clamp(0, maxValue) / maxValue;
			return true;
		}
		return false;
	}

	private static bool TryParseOpacity(ReadOnlySpan<char> elem, out double value)
	{
		if (TryParseDouble(elem, out value))
		{
			value = value.Clamp(0, 1);
			return true;
		}
		return false;
	}

	private static bool TryParseDouble(ReadOnlySpan<char> s, out double value) =>
		double.TryParse(
#if NETSTANDARD2_0 || TIZEN
			s.ToString(),
#else
			s,
#endif
			NumberStyles.Number, CultureInfo.InvariantCulture, out value);

	private static int ParseInt(ReadOnlySpan<char> s) =>
		int.Parse(
#if NETSTANDARD2_0 || TIZEN
			s.ToString(),
#else
			s,
#endif
			 NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
}
