using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Graphics
{
	[DebuggerDisplay("Red={Red}, Green={Green}, Blue={Blue}, Alpha={Alpha}")]
	[TypeConverter(typeof(ColorTypeConverter))]
	public class Color
	{
		public readonly float Red;
		public readonly float Green;
		public readonly float Blue;
		public readonly float Alpha = 1;

		public Color()
		{
			// Default Black
			Red = Green = Blue = 0;	
		}

		public Color(float gray)
		{
			Red = Green = Blue = gray.Clamp(0, 1);
		}

		public Color(float red, float green, float blue)
		{
			Red = red.Clamp(0, 1);
			Green = green.Clamp(0, 1);
			Blue = blue.Clamp(0, 1);
		}

		public Color(float red, float green, float blue, float alpha)
		{
			Red = red.Clamp(0, 1);
			Green = green.Clamp(0, 1);
			Blue = blue.Clamp(0, 1);
			Alpha = alpha.Clamp(0, 1);
		}

		public override string ToString()
		{
			return $"[Color: Red={Red}, Green={Green}, Blue={Blue}, Alpha={Alpha}]";
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashcode = Red.GetHashCode();
				hashcode = (hashcode * 397) ^ Green.GetHashCode();
				hashcode = (hashcode * 397) ^ Blue.GetHashCode();
				hashcode = (hashcode * 397) ^ Alpha.GetHashCode();
				return hashcode;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is Color other)
				return NearlyEqual(Red, other.Red)
					&& NearlyEqual(Green, other.Green)
					&& NearlyEqual(Blue, other.Blue)
					&& NearlyEqual(Alpha, other.Alpha);

			return base.Equals(obj);
		}

		static bool NearlyEqual(float f1, float f2, float epsilon = 0.01f)
			=> Math.Abs(f1 - f2) < epsilon;

		[Obsolete("Use ToArgbHex instead.")]
		public string ToHex(bool includeAlpha)
		{
			if (includeAlpha || Alpha < 1)
				return "#" + ToHex(Alpha) + ToHex(Red) + ToHex(Green) + ToHex(Blue);

			return "#" + ToHex(Red) + ToHex(Green) + ToHex(Blue);
		}

		public string ToHex()
		{
			return "#" + ToHex(Red) + ToHex(Green) + ToHex(Blue);
		}

		public string ToArgbHex(bool includeAlpha = false)
		{
			if (includeAlpha || Alpha < 1)
				return "#" + ToHex(Alpha) + ToHex(Red) + ToHex(Green) + ToHex(Blue);

			return "#" + ToHex(Red) + ToHex(Green) + ToHex(Blue);
		}

		public string ToRgbaHex(bool includeAlpha = false)
		{
			if (includeAlpha || Alpha < 1)
				return "#" + ToHex(Red) + ToHex(Green) + ToHex(Blue) + ToHex(Alpha);

			return "#" + ToHex(Red) + ToHex(Green) + ToHex(Blue);
		}

		[Obsolete("Use FromArgb instead.")]
		public static Color FromHex(string colorAsArgbHex) => FromArgb(colorAsArgbHex);

		public Paint AsPaint()
		{
			return new SolidPaint()
			{
				Color = this
			};
		}

		public Color WithAlpha(float alpha)
		{
			if (Math.Abs(alpha - Alpha) < Geometry.Epsilon)
				return this;

			return new Color(Red, Green, Blue, alpha);
		}

		public Color MultiplyAlpha(float multiplyBy)
		{
			return new Color(Red, Green, Blue, Alpha * multiplyBy);
		}

		private static string ToHex(float value)
		{
			var intValue = (int)(255f * value);
			var stringValue = intValue.ToString("X");
			if (stringValue.Length == 1)
				return "0" + stringValue;

			return stringValue;
		}

		public int ToInt()
		{
			ToRgba(out var r, out var g, out var b, out var a);
			int argb = a << 24 | r << 16 | g << 8 | b;
			return argb;
		}

		public uint ToUint() => (uint)ToInt();

		public void ToRgb(out byte r, out byte g, out byte b) =>
			ToRgba(out r, out g, out b, out _);

		public void ToRgba(out byte r, out byte g, out byte b, out byte a)
		{
			a = (byte)(Alpha * 255f);
			r = (byte)(Red * 255f);
			g = (byte)(Green * 255f);
			b = (byte)(Blue * 255f);
		}

		public float GetLuminosity()
		{
			float v = Math.Max(Red, Green);
			v = Math.Max(v, Blue);
			float m = Math.Min(Red, Green);
			m = Math.Min(m, Blue);
			var l = (m + v) / 2.0f;
			if (l <= 0.0)
				return 0;
			return l;
		}

		public Color AddLuminosity(float delta)
		{
			ToHsl(out var h, out var s, out var l);
			l += delta;
			l = l.Clamp(0, 1);
			return FromHsla(h, s, l, Alpha);
		}

		public Color WithLuminosity(float luminosity)
		{
			ToHsl(out var h, out var s, out var l);
			return FromHsla(h, s, luminosity, Alpha);
		}

		public float GetSaturation()
		{
			ToHsl(out var h, out var s, out var l);
			return s;
		}

		public Color WithSaturation(float saturation)
		{
			ToHsl(out var h, out var s, out var l);
			return FromHsla(h, saturation, l, Alpha);
		}

		public float GetHue()
		{
			ToHsl(out var h, out var s, out var l);
			return h;
		}

		public Color WithHue(float hue)
		{
			ToHsl(out var h, out var s, out var l);
			return FromHsla(hue, s, l, Alpha);
		}

		public Color GetComplementary()
		{
			ToHsl(out var h, out var s, out var l);

			// Add 180 (degrees) to get to the other side of the circle.
			h += 0.5f;

			// Ensure still within the bounds of a circle.
			h %= 1.0f;

			return Color.FromHsla(h, s, l);
		}

		public static Color FromHsva(float h, float s, float v, float a)
		{
			h = h.Clamp(0, 1);
			s = s.Clamp(0, 1);
			v = v.Clamp(0, 1);
			var range = (int)(Math.Floor(h * 6)) % 6;
			var f = h * 6 - Math.Floor(h * 6);
			var p = v * (1 - s);
			var q = v * (1 - f * s);
			var t = v * (1 - (1 - f) * s);

			switch (range)
			{
				case 0:
					return FromRgba(v, t, p, a);
				case 1:
					return FromRgba(q, v, p, a);
				case 2:
					return FromRgba(p, v, t, a);
				case 3:
					return FromRgba(p, q, v, a);
				case 4:
					return FromRgba(t, p, v, a);
			}
			return FromRgba(v, p, q, a);
		}

		public static Color FromUint(uint argb)
		{
			return FromRgba((byte)((argb & 0x00ff0000) >> 0x10), (byte)((argb & 0x0000ff00) >> 0x8), (byte)(argb & 0x000000ff), (byte)((argb & 0xff000000) >> 0x18));
		}

		public static Color FromInt(int argb)
		{
			return FromRgba((byte)((argb & 0x00ff0000) >> 0x10), (byte)((argb & 0x0000ff00) >> 0x8), (byte)(argb & 0x000000ff), (byte)((argb & 0xff000000) >> 0x18));
		}

		public static Color FromRgb(byte red, byte green, byte blue)
		{
			return Color.FromRgba(red, green, blue, 255);
		}

		public static Color FromRgba(byte red, byte green, byte blue, byte alpha)
		{
			return new Color(red / 255f, green / 255f, blue / 255f, alpha / 255f);
		}

		public static Color FromRgb(float red, float green, float blue)
		{
			return Color.FromRgba(red, green, blue, 1);
		}

		public static Color FromRgb(double red, double green, double blue)
		{
			return Color.FromRgba(red, green, blue, 1);
		}

		public static Color FromRgba(float r, float g, float b, float a)
		{
			return new Color(r, g, b, a);
		}

		public static Color FromRgba(double r, double g, double b, double a)
		{
			return new Color((float)r, (float)g, (float)b, (float)a);
		}

		public static Color FromRgba(string colorAsHex)
		{
			//Remove # if present
			if (colorAsHex.IndexOf('#') != -1)
				colorAsHex = colorAsHex.Replace("#", "");

			int red = 0;
			int green = 0;
			int blue = 0;
			int alpha = 255;

			if (colorAsHex.Length == 6)
			{
				//#RRGGBB
				red = int.Parse(colorAsHex.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse(colorAsHex.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse(colorAsHex.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 3)
			{
				//#RGB
				red = int.Parse($"{colorAsHex[0]}{colorAsHex[0]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse($"{colorAsHex[1]}{colorAsHex[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse($"{colorAsHex[2]}{colorAsHex[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 4)
			{
				//#RGBA
				red = int.Parse($"{colorAsHex[0]}{colorAsHex[0]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse($"{colorAsHex[1]}{colorAsHex[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse($"{colorAsHex[2]}{colorAsHex[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				alpha = int.Parse($"{colorAsHex[3]}{colorAsHex[3]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 8)
			{
				//#RRGGBBAA
				red = int.Parse(colorAsHex.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse(colorAsHex.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse(colorAsHex.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				alpha = int.Parse(colorAsHex.Substring(6, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}

			return FromRgba(red / 255f, green / 255f, blue / 255f, alpha / 255f);
		}

		public static Color FromArgb(string colorAsHex)
		{
			//Remove # if present
			if (colorAsHex.IndexOf('#') != -1)
				colorAsHex = colorAsHex.Replace("#", "");

			int red = 0;
			int green = 0;
			int blue = 0;
			int alpha = 255;

			if (colorAsHex.Length == 6)
			{
				//#RRGGBB
				red = int.Parse(colorAsHex.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse(colorAsHex.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse(colorAsHex.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 3)
			{
				//#RGB
				red = int.Parse($"{colorAsHex[0]}{colorAsHex[0]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse($"{colorAsHex[1]}{colorAsHex[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse($"{colorAsHex[2]}{colorAsHex[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 4)
			{
				//#ARGB
				alpha = int.Parse($"{colorAsHex[0]}{colorAsHex[0]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				red = int.Parse($"{colorAsHex[1]}{colorAsHex[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse($"{colorAsHex[2]}{colorAsHex[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse($"{colorAsHex[3]}{colorAsHex[3]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 8)
			{
				//#AARRGGBB
				alpha = int.Parse(colorAsHex.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				red = int.Parse(colorAsHex.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse(colorAsHex.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse(colorAsHex.Substring(6, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}

			return FromRgba(red / 255f, green / 255f, blue / 255f, alpha / 255f);
		}

		public static Color FromHsla(float h, float s, float l, float a = 1)
		{
			float red, green, blue;
			ConvertToRgb(h, s, l, out red, out green, out blue);
			return new Color(red, green, blue, a);
		}

		public static Color FromHsla(double h, double s, double l, double a = 1)
		{
			float red, green, blue;
			ConvertToRgb((float)h, (float)s, (float)l, out red, out green, out blue);
			return new Color(red, green, blue, (float)a);
		}

		public static Color FromHsv(float h, float s, float v)
		{
			return FromHsva(h, s, v, 1f);
		}

		public static Color FromHsva(int h, int s, int v, int a)
		{
			return FromHsva(h / 360f, s / 100f, v / 100f, a / 100f);
		}

		public static Color FromHsv(int h, int s, int v)
		{
			return FromHsva(h / 360f, s / 100f, v / 100f, 1f);
		}

		private static void ConvertToRgb(float hue, float saturation, float luminosity, out float r, out float g, out float b)
		{
			if (luminosity == 0)
			{
				r = g = b = 0;
				return;
			}

			if (saturation == 0)
			{
				r = g = b = luminosity;
				return;
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

			r = clr[0];
			g = clr[1];
			b = clr[2];
		}

		public void ToHsl(out float h, out float s, out float l)
		{
			var r = Red;
			var g = Green;
			var b = Blue;

			float v = Math.Max(r, g);
			v = Math.Max(v, b);

			float m = Math.Min(r, g);
			m = Math.Min(m, b);

			l = (m + v) / 2.0f;
			if (l <= 0.0)
			{
				h = s = l = 0;
				return;
			}
			float vm = v - m;
			s = vm;

			if (s > 0.0)
			{
				s /= l <= 0.5f ? v + m : 2.0f - v - m;
			}
			else
			{
				h = 0;
				s = 0;
				return;
			}

			float r2 = (v - r) / vm;
			float g2 = (v - g) / vm;
			float b2 = (v - b) / vm;

			if (r == v)
			{
				h = g == m ? 5.0f + b2 : 1.0f - g2;
			}
			else if (g == v)
			{
				h = b == m ? 1.0f + r2 : 3.0f - b2;
			}
			else
			{
				h = r == m ? 3.0f + g2 : 5.0f - r2;
			}
			h /= 6.0f;
		}


		// Supported inputs
		// HEX		#rgb, #argb, #rrggbb, #aarrggbb
		// RGB		rgb(255,0,0), rgb(100%,0%,0%)					values in range 0-255 or 0%-100%
		// RGBA		rgba(255, 0, 0, 0.8), rgba(100%, 0%, 0%, 0.8)	opacity is 0.0-1.0
		// HSL		hsl(120, 100%, 50%)								h is 0-360, s and l are 0%-100%
		// HSLA		hsla(120, 100%, 50%, .8)						opacity is 0.0-1.0
		// HSV		hsv(120, 100%, 50%)								h is 0-360, s and v are 0%-100%
		// HSVA		hsva(120, 100%, 50%, .8)						opacity is 0.0-1.0
		// Predefined color											case insensitive
		public static Color Parse(string value)
		{
			static Exception ConvertException(string inputValue)
				=> new InvalidOperationException($"Cannot convert \"{inputValue}\" into {typeof(Color)}");

			if (value != null)
			{
				value = value.Trim();
				if (value.StartsWith("#", StringComparison.Ordinal))
					return Color.FromHex(value);

				if (value.StartsWith("rgba", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw ConvertException(value);

					var quad = value.Substring(op + 1, cp - op - 1).Split(',');
					if (quad.Length != 4)
						throw ConvertException(value);

					var r = ParseColorValue(quad[0], 255, acceptPercent: true);
					var g = ParseColorValue(quad[1], 255, acceptPercent: true);
					var b = ParseColorValue(quad[2], 255, acceptPercent: true);
					var a = ParseOpacity(quad[3]);

					return new Color((float)r, (float)g, (float)b, (float)a);
				}

				if (value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');

					if (op < 0 || cp < 0 || cp < op)
						throw ConvertException(value);

					var triplet = value.Substring(op + 1, cp - op - 1).Split(',');
					if (triplet.Length != 3)
						throw ConvertException(value);

					var r = ParseColorValue(triplet[0], 255, acceptPercent: true);
					var g = ParseColorValue(triplet[1], 255, acceptPercent: true);
					var b = ParseColorValue(triplet[2], 255, acceptPercent: true);

					return new Color((float)r, (float)g, (float)b);
				}

				if (value.StartsWith("hsla", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw ConvertException(value);

					var quad = value.Substring(op + 1, cp - op - 1).Split(',');
					if (quad.Length != 4)
						throw ConvertException(value);

					var h = ParseColorValue(quad[0], 360, acceptPercent: false);
					var s = ParseColorValue(quad[1], 100, acceptPercent: true);
					var l = ParseColorValue(quad[2], 100, acceptPercent: true);
					var a = ParseOpacity(quad[3]);

					return Color.FromHsla(h, s, l, a);
				}

				if (value.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw ConvertException(value);

					var triplet = value.Substring(op + 1, cp - op - 1).Split(',');
					if (triplet.Length != 3)
						throw ConvertException(value);

					var h = ParseColorValue(triplet[0], 360, acceptPercent: false);
					var s = ParseColorValue(triplet[1], 100, acceptPercent: true);
					var l = ParseColorValue(triplet[2], 100, acceptPercent: true);

					return Color.FromHsla(h, s, l);
				}

				if (value.StartsWith("hsva", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw ConvertException(value);

					var quad = value.Substring(op + 1, cp - op - 1).Split(',');
					if (quad.Length != 4)
						throw ConvertException(value);

					var h = ParseColorValue(quad[0], 360, acceptPercent: false);
					var s = ParseColorValue(quad[1], 100, acceptPercent: true);
					var v = ParseColorValue(quad[2], 100, acceptPercent: true);
					var a = ParseOpacity(quad[3]);

					return Color.FromHsva((float)h, (float)s, (float)v, (float)a);
				}

				if (value.StartsWith("hsv", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw ConvertException(value);

					var triplet = value.Substring(op + 1, cp - op - 1).Split(',');
					if (triplet.Length != 3)
						throw ConvertException(value);

					var h = ParseColorValue(triplet[0], 360, acceptPercent: false);
					var s = ParseColorValue(triplet[1], 100, acceptPercent: true);
					var v = ParseColorValue(triplet[2], 100, acceptPercent: true);

					return Color.FromHsv((float)h, (float)s, (float)v);
				}

				string[] parts = value.Split('.');
				if (parts.Length == 1 || (parts.Length == 2 && parts[0] == "Color"))
				{
					string color = parts[parts.Length - 1];
					switch (color.ToLowerInvariant())
					{
						case "default":
							return null;
						//TODO: Remove this hack. Colors.Accent bad!
						//case "accent":
						//	return Colors.Accent;
						case "aliceblue":
							return Colors.AliceBlue;
						case "antiquewhite":
							return Colors.AntiqueWhite;
						case "aqua":
							return Colors.Aqua;
						case "aquamarine":
							return Colors.Aquamarine;
						case "azure":
							return Colors.Azure;
						case "beige":
							return Colors.Beige;
						case "bisque":
							return Colors.Bisque;
						case "black":
							return Colors.Black;
						case "blanchedalmond":
							return Colors.BlanchedAlmond;
						case "blue":
							return Colors.Blue;
						case "blueViolet":
							return Colors.BlueViolet;
						case "brown":
							return Colors.Brown;
						case "burlywood":
							return Colors.BurlyWood;
						case "cadetblue":
							return Colors.CadetBlue;
						case "chartreuse":
							return Colors.Chartreuse;
						case "chocolate":
							return Colors.Chocolate;
						case "coral":
							return Colors.Coral;
						case "cornflowerblue":
							return Colors.CornflowerBlue;
						case "cornsilk":
							return Colors.Cornsilk;
						case "crimson":
							return Colors.Crimson;
						case "cyan":
							return Colors.Cyan;
						case "darkblue":
							return Colors.DarkBlue;
						case "darkcyan":
							return Colors.DarkCyan;
						case "darkgoldenrod":
							return Colors.DarkGoldenrod;
						case "darkgray":
							return Colors.DarkGray;
						case "darkgreen":
							return Colors.DarkGreen;
						case "darkkhaki":
							return Colors.DarkKhaki;
						case "darkmagenta":
							return Colors.DarkMagenta;
						case "darkolivegreen":
							return Colors.DarkOliveGreen;
						case "darkorange":
							return Colors.DarkOrange;
						case "darkorchid":
							return Colors.DarkOrchid;
						case "darkred":
							return Colors.DarkRed;
						case "darksalmon":
							return Colors.DarkSalmon;
						case "darkseagreen":
							return Colors.DarkSeaGreen;
						case "darkslateblue":
							return Colors.DarkSlateBlue;
						case "darkslategray":
							return Colors.DarkSlateGray;
						case "darkturquoise":
							return Colors.DarkTurquoise;
						case "darkviolet":
							return Colors.DarkViolet;
						case "deeppink":
							return Colors.DeepPink;
						case "deepskyblue":
							return Colors.DeepSkyBlue;
						case "dimgray":
							return Colors.DimGray;
						case "dodgerblue":
							return Colors.DodgerBlue;
						case "firebrick":
							return Colors.Firebrick;
						case "floralwhite":
							return Colors.FloralWhite;
						case "forestgreen":
							return Colors.ForestGreen;
						case "fuchsia":
							return Colors.Fuchsia;
						case "gainsboro":
							return Colors.Gainsboro;
						case "ghostwhite":
							return Colors.GhostWhite;
						case "gold":
							return Colors.Gold;
						case "goldenrod":
							return Colors.Goldenrod;
						case "gray":
							return Colors.Gray;
						case "green":
							return Colors.Green;
						case "greenyellow":
							return Colors.GreenYellow;
						case "honeydew":
							return Colors.Honeydew;
						case "hotpink":
							return Colors.HotPink;
						case "indianred":
							return Colors.IndianRed;
						case "indigo":
							return Colors.Indigo;
						case "ivory":
							return Colors.Ivory;
						case "khaki":
							return Colors.Khaki;
						case "lavender":
							return Colors.Lavender;
						case "lavenderblush":
							return Colors.LavenderBlush;
						case "lawngreen":
							return Colors.LawnGreen;
						case "lemonchiffon":
							return Colors.LemonChiffon;
						case "lightblue":
							return Colors.LightBlue;
						case "lightcoral":
							return Colors.LightCoral;
						case "lightcyan":
							return Colors.LightCyan;
						case "lightgoldenrodyellow":
							return Colors.LightGoldenrodYellow;
						case "lightgrey":
						case "lightgray":
							return Colors.LightGray;
						case "lightgreen":
							return Colors.LightGreen;
						case "lightpink":
							return Colors.LightPink;
						case "lightsalmon":
							return Colors.LightSalmon;
						case "lightseagreen":
							return Colors.LightSeaGreen;
						case "lightskyblue":
							return Colors.LightSkyBlue;
						case "lightslategray":
							return Colors.LightSlateGray;
						case "lightsteelblue":
							return Colors.LightSteelBlue;
						case "lightyellow":
							return Colors.LightYellow;
						case "lime":
							return Colors.Lime;
						case "limegreen":
							return Colors.LimeGreen;
						case "linen":
							return Colors.Linen;
						case "magenta":
							return Colors.Magenta;
						case "maroon":
							return Colors.Maroon;
						case "mediumaquamarine":
							return Colors.MediumAquamarine;
						case "mediumblue":
							return Colors.MediumBlue;
						case "mediumorchid":
							return Colors.MediumOrchid;
						case "mediumpurple":
							return Colors.MediumPurple;
						case "mediumseagreen":
							return Colors.MediumSeaGreen;
						case "mediumslateblue":
							return Colors.MediumSlateBlue;
						case "mediumspringgreen":
							return Colors.MediumSpringGreen;
						case "mediumturquoise":
							return Colors.MediumTurquoise;
						case "mediumvioletred":
							return Colors.MediumVioletRed;
						case "midnightblue":
							return Colors.MidnightBlue;
						case "mintcream":
							return Colors.MintCream;
						case "mistyrose":
							return Colors.MistyRose;
						case "moccasin":
							return Colors.Moccasin;
						case "navajowhite":
							return Colors.NavajoWhite;
						case "navy":
							return Colors.Navy;
						case "oldlace":
							return Colors.OldLace;
						case "olive":
							return Colors.Olive;
						case "olivedrab":
							return Colors.OliveDrab;
						case "orange":
							return Colors.Orange;
						case "orangered":
							return Colors.OrangeRed;
						case "orchid":
							return Colors.Orchid;
						case "palegoldenrod":
							return Colors.PaleGoldenrod;
						case "palegreen":
							return Colors.PaleGreen;
						case "paleturquoise":
							return Colors.PaleTurquoise;
						case "palevioletred":
							return Colors.PaleVioletRed;
						case "papayawhip":
							return Colors.PapayaWhip;
						case "peachpuff":
							return Colors.PeachPuff;
						case "peru":
							return Colors.Peru;
						case "pink":
							return Colors.Pink;
						case "plum":
							return Colors.Plum;
						case "powderblue":
							return Colors.PowderBlue;
						case "purple":
							return Colors.Purple;
						case "red":
							return Colors.Red;
						case "rosybrown":
							return Colors.RosyBrown;
						case "royalblue":
							return Colors.RoyalBlue;
						case "saddlebrown":
							return Colors.SaddleBrown;
						case "salmon":
							return Colors.Salmon;
						case "sandybrown":
							return Colors.SandyBrown;
						case "seagreen":
							return Colors.SeaGreen;
						case "seashell":
							return Colors.SeaShell;
						case "sienna":
							return Colors.Sienna;
						case "silver":
							return Colors.Silver;
						case "skyblue":
							return Colors.SkyBlue;
						case "slateblue":
							return Colors.SlateBlue;
						case "slategray":
							return Colors.SlateGray;
						case "snow":
							return Colors.Snow;
						case "springgreen":
							return Colors.SpringGreen;
						case "steelblue":
							return Colors.SteelBlue;
						case "tan":
							return Colors.Tan;
						case "teal":
							return Colors.Teal;
						case "thistle":
							return Colors.Thistle;
						case "tomato":
							return Colors.Tomato;
						case "transparent":
							return Colors.Transparent;
						case "turquoise":
							return Colors.Turquoise;
						case "violet":
							return Colors.Violet;
						case "wheat":
							return Colors.Wheat;
						case "white":
							return Colors.White;
						case "whitesmoke":
							return Colors.WhiteSmoke;
						case "yellow":
							return Colors.Yellow;
						case "yellowgreen":
							return Colors.YellowGreen;
					}

					// Look for fields of Color or Colors matching the name
					var field = typeof(Color).GetFields().FirstOrDefault(fi => fi.IsStatic && string.Equals(fi.Name, color, StringComparison.OrdinalIgnoreCase));
					if (field != null)
						return (Color)field.GetValue(null);
					field = typeof(Colors).GetFields().FirstOrDefault(fi => fi.IsStatic && string.Equals(fi.Name, color, StringComparison.OrdinalIgnoreCase));
					if (field != null)
						return (Color)field.GetValue(null);

					// Look for property of Color or Colors matching the name
					var property = typeof(Color).GetProperties().FirstOrDefault(pi => string.Equals(pi.Name, color, StringComparison.OrdinalIgnoreCase) && pi.CanRead && pi.GetMethod.IsStatic);
					if (property != null)
						return (Color)property.GetValue(null, null);
					property = typeof(Colors).GetProperties().FirstOrDefault(pi => string.Equals(pi.Name, color, StringComparison.OrdinalIgnoreCase) && pi.CanRead && pi.GetMethod.IsStatic);
					if (property != null)
						return (Color)property.GetValue(null, null);
				}
			}

			throw ConvertException(value);
		}

		static double ParseColorValue(string elem, int maxValue, bool acceptPercent)
		{
			elem = elem.Trim();
			if (elem.EndsWith("%", StringComparison.Ordinal) && acceptPercent)
			{
				maxValue = 100;
				elem = elem.Substring(0, elem.Length - 1);
			}
			return double.Parse(elem, NumberStyles.Number, CultureInfo.InvariantCulture).Clamp(0, maxValue) / maxValue;
		}

		static double ParseOpacity(string elem)
			=> double.Parse(elem, NumberStyles.Number, CultureInfo.InvariantCulture).Clamp(0, 1);

	}
}
