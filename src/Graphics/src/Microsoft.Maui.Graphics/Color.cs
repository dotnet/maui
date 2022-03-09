using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	[DebuggerDisplay("Red={Red}, Green={Green}, Blue={Blue}, Alpha={Alpha}")]
	[TypeConverter(typeof(Converters.ColorTypeConverter))]
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

		public Color(byte red, byte green, byte blue)
		{
			Red = (red / 255f).Clamp(0, 1);
			Green = (green / 255f).Clamp(0, 1);
			Blue = (blue / 255f).Clamp(0, 1);
			Alpha = 1.0f;
		}

		public Color(byte red, byte green, byte blue, byte alpha)
		{
			Red = (red / 255f).Clamp(0, 1);
			Green = (green / 255f).Clamp(0, 1);
			Blue = (blue / 255f).Clamp(0, 1);
			Alpha = (alpha / 255f).Clamp(0, 1);
		}

		public Color(int red, int green, int blue)
		{
			Red = (red / 255f).Clamp(0, 1);
			Green = (green / 255f).Clamp(0, 1);
			Blue = (blue / 255f).Clamp(0, 1);
			Alpha = 1.0f;
		}

		public Color(int red, int green, int blue, int alpha)
		{
			Red = (red / 255f).Clamp(0, 1);
			Green = (green / 255f).Clamp(0, 1);
			Blue = (blue / 255f).Clamp(0, 1);
			Alpha = (alpha / 255f).Clamp(0, 1);
		}

		public Color(Vector4 color)
		{
			Red = color.X.Clamp(0, 1);
			Green = color.Y.Clamp(0, 1);
			Blue = color.Z.Clamp(0, 1);
			Alpha = color.W.Clamp(0, 1);
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
			return new Color(red / 255f, green / 255f, blue / 255f, 1f);
		}

		public static Color FromRgba(byte red, byte green, byte blue, byte alpha)
		{
			return new Color(red / 255f, green / 255f, blue / 255f, alpha / 255f);
		}

		public static Color FromRgb(int red, int green, int blue)
		{
			return new Color(red / 255f, green / 255f, blue / 255f, 1f);
		}

		public static Color FromRgba(int red, int green, int blue, int alpha)
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

		public static Color FromArgb(string colorAsHex) => FromArgb(colorAsHex != null ? colorAsHex.AsSpan() : default);

		static Color FromArgb(ReadOnlySpan<char> colorAsHex)
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
			if (TryParse(value, out var c) && c != default)
				return c;

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
		}

		public static bool TryParse(string value, out Color color) => TryParse(value != null ? value.AsSpan() : default, out color);

		static bool TryParse(ReadOnlySpan<char> value, out Color color)
		{
			value = value.Trim();
			if (!value.IsEmpty)
			{
				if (value[0] == '#')
				{
					try
					{
						color = Color.FromArgb(value);
						return true;
					}
					catch
					{
						color = default;
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
						color = default;
						return false;
					}

					var r = ParseColorValue(quad0, 255, acceptPercent: true);
					var g = ParseColorValue(quad1, 255, acceptPercent: true);
					var b = ParseColorValue(quad2, 255, acceptPercent: true);
					var a = ParseOpacity(quad3);

					color = new Color((float)r, (float)g, (float)b, (float)a);
					return true;
				}

				if (value.StartsWith("rgb".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					if (!TryParseThreeColorRanges(value,
						out ReadOnlySpan<char> triplet0,
						out ReadOnlySpan<char> triplet1,
						out ReadOnlySpan<char> triplet2))
					{
						color = default;
						return false;
					}

					var r = ParseColorValue(triplet0, 255, acceptPercent: true);
					var g = ParseColorValue(triplet1, 255, acceptPercent: true);
					var b = ParseColorValue(triplet2, 255, acceptPercent: true);

					color = new Color((float)r, (float)g, (float)b);
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
						color = default;
						return false;
					}

					var h = ParseColorValue(quad0, 360, acceptPercent: false);
					var s = ParseColorValue(quad1, 100, acceptPercent: true);
					var l = ParseColorValue(quad2, 100, acceptPercent: true);
					var a = ParseOpacity(quad3);

					color = Color.FromHsla(h, s, l, a);
					return true;
				}

				if (value.StartsWith("hsl".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					if (!TryParseThreeColorRanges(value,
						out ReadOnlySpan<char> triplet0,
						out ReadOnlySpan<char> triplet1,
						out ReadOnlySpan<char> triplet2))
					{
						color = default;
						return false;
					}

					var h = ParseColorValue(triplet0, 360, acceptPercent: false);
					var s = ParseColorValue(triplet1, 100, acceptPercent: true);
					var l = ParseColorValue(triplet2, 100, acceptPercent: true);

					color = Color.FromHsla(h, s, l);
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
						color = default;
						return false;
					}

					var h = ParseColorValue(quad0, 360, acceptPercent: false);
					var s = ParseColorValue(quad1, 100, acceptPercent: true);
					var v = ParseColorValue(quad2, 100, acceptPercent: true);
					var a = ParseOpacity(quad3);

					color = Color.FromHsva((float)h, (float)s, (float)v, (float)a);
					return true;
				}

				if (value.StartsWith("hsv".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					if (!TryParseThreeColorRanges(value,
						out ReadOnlySpan<char> triplet0,
						out ReadOnlySpan<char> triplet1,
						out ReadOnlySpan<char> triplet2))
					{
						color = default;
						return false;
					}

					var h = ParseColorValue(triplet0, 360, acceptPercent: false);
					var s = ParseColorValue(triplet1, 100, acceptPercent: true);
					var v = ParseColorValue(triplet2, 100, acceptPercent: true);

					color = Color.FromHsv((float)h, (float)s, (float)v);
					return true;
				}

				var namedColor = GetNamedColor(value);
				if (namedColor != null)
				{
					color = namedColor;
					return true;
				}
			}

			color = default;
			return false;
		}

		static Color GetNamedColor(ReadOnlySpan<char> value)
		{
			if (s_namedColors.TryGetValue(ComputeIgnoreCaseStringHash(value), out (string, Color) entry))
			{
				if (value.Equals(entry.Item1.AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					return entry.Item2;
				}
			}

			return null;
		}

		static readonly Dictionary<uint, (string, Color)> s_namedColors = BuildNamedColorDictionary();

		static Dictionary<uint, (string, Color)> BuildNamedColorDictionary()
		{
			Dictionary<uint, (string, Color)> namedColors = new Dictionary<uint, (string, Color)>(capacity: 149);
			void AddColor(string name, Color color)
			{
				uint hash = ComputeIgnoreCaseStringHash(name.AsSpan());

				// If we encounter a hash-collison on the known color names, we will need to update our hashing algorithm
				Debug.Assert(!namedColors.ContainsKey(hash));
				namedColors.Add(hash, (name, color));
			}

			AddColor("default", default);
			AddColor("aliceblue", Colors.AliceBlue);
			AddColor("antiquewhite", Colors.AntiqueWhite);
			AddColor("aqua", Colors.Aqua);
			AddColor("aquamarine", Colors.Aquamarine);
			AddColor("azure", Colors.Azure);
			AddColor("beige", Colors.Beige);
			AddColor("bisque", Colors.Bisque);
			AddColor("black", Colors.Black);
			AddColor("blanchedalmond", Colors.BlanchedAlmond);
			AddColor("blue", Colors.Blue);
			AddColor("blueviolet", Colors.BlueViolet);
			AddColor("brown", Colors.Brown);
			AddColor("burlywood", Colors.BurlyWood);
			AddColor("cadetblue", Colors.CadetBlue);
			AddColor("chartreuse", Colors.Chartreuse);
			AddColor("chocolate", Colors.Chocolate);
			AddColor("coral", Colors.Coral);
			AddColor("cornflowerblue", Colors.CornflowerBlue);
			AddColor("cornsilk", Colors.Cornsilk);
			AddColor("crimson", Colors.Crimson);
			AddColor("cyan", Colors.Cyan);
			AddColor("darkblue", Colors.DarkBlue);
			AddColor("darkcyan", Colors.DarkCyan);
			AddColor("darkgoldenrod", Colors.DarkGoldenrod);
			AddColor("darkgray", Colors.DarkGray);
			AddColor("darkgreen", Colors.DarkGreen);
			AddColor("darkkhaki", Colors.DarkKhaki);
			AddColor("darkmagenta", Colors.DarkMagenta);
			AddColor("darkolivegreen", Colors.DarkOliveGreen);
			AddColor("darkorange", Colors.DarkOrange);
			AddColor("darkorchid", Colors.DarkOrchid);
			AddColor("darkred", Colors.DarkRed);
			AddColor("darksalmon", Colors.DarkSalmon);
			AddColor("darkseagreen", Colors.DarkSeaGreen);
			AddColor("darkslateblue", Colors.DarkSlateBlue);
			AddColor("darkslategray", Colors.DarkSlateGray);
			AddColor("darkturquoise", Colors.DarkTurquoise);
			AddColor("darkviolet", Colors.DarkViolet);
			AddColor("deeppink", Colors.DeepPink);
			AddColor("deepskyblue", Colors.DeepSkyBlue);
			AddColor("dimgray", Colors.DimGray);
			AddColor("dodgerblue", Colors.DodgerBlue);
			AddColor("firebrick", Colors.Firebrick);
			AddColor("floralwhite", Colors.FloralWhite);
			AddColor("forestgreen", Colors.ForestGreen);
			AddColor("fuchsia", Colors.Fuchsia);
			AddColor("gainsboro", Colors.Gainsboro);
			AddColor("ghostwhite", Colors.GhostWhite);
			AddColor("gold", Colors.Gold);
			AddColor("goldenrod", Colors.Goldenrod);
			AddColor("gray", Colors.Gray);
			AddColor("green", Colors.Green);
			AddColor("greenyellow", Colors.GreenYellow);
			AddColor("honeydew", Colors.Honeydew);
			AddColor("hotpink", Colors.HotPink);
			AddColor("indianred", Colors.IndianRed);
			AddColor("indigo", Colors.Indigo);
			AddColor("ivory", Colors.Ivory);
			AddColor("khaki", Colors.Khaki);
			AddColor("lavender", Colors.Lavender);
			AddColor("lavenderblush", Colors.LavenderBlush);
			AddColor("lawngreen", Colors.LawnGreen);
			AddColor("lemonchiffon", Colors.LemonChiffon);
			AddColor("lightblue", Colors.LightBlue);
			AddColor("lightcoral", Colors.LightCoral);
			AddColor("lightcyan", Colors.LightCyan);
			AddColor("lightgoldenrodyellow", Colors.LightGoldenrodYellow);
			AddColor("lightgrey", Colors.LightGrey);
			AddColor("lightgray", Colors.LightGray);
			AddColor("lightgreen", Colors.LightGreen);
			AddColor("lightpink", Colors.LightPink);
			AddColor("lightsalmon", Colors.LightSalmon);
			AddColor("lightseagreen", Colors.LightSeaGreen);
			AddColor("lightskyblue", Colors.LightSkyBlue);
			AddColor("lightslategray", Colors.LightSlateGray);
			AddColor("lightsteelblue", Colors.LightSteelBlue);
			AddColor("lightyellow", Colors.LightYellow);
			AddColor("lime", Colors.Lime);
			AddColor("limegreen", Colors.LimeGreen);
			AddColor("linen", Colors.Linen);
			AddColor("magenta", Colors.Magenta);
			AddColor("maroon", Colors.Maroon);
			AddColor("mediumaquamarine", Colors.MediumAquamarine);
			AddColor("mediumblue", Colors.MediumBlue);
			AddColor("mediumorchid", Colors.MediumOrchid);
			AddColor("mediumpurple", Colors.MediumPurple);
			AddColor("mediumseagreen", Colors.MediumSeaGreen);
			AddColor("mediumslateblue", Colors.MediumSlateBlue);
			AddColor("mediumspringgreen", Colors.MediumSpringGreen);
			AddColor("mediumturquoise", Colors.MediumTurquoise);
			AddColor("mediumvioletred", Colors.MediumVioletRed);
			AddColor("midnightblue", Colors.MidnightBlue);
			AddColor("mintcream", Colors.MintCream);
			AddColor("mistyrose", Colors.MistyRose);
			AddColor("moccasin", Colors.Moccasin);
			AddColor("navajowhite", Colors.NavajoWhite);
			AddColor("navy", Colors.Navy);
			AddColor("oldlace", Colors.OldLace);
			AddColor("olive", Colors.Olive);
			AddColor("olivedrab", Colors.OliveDrab);
			AddColor("orange", Colors.Orange);
			AddColor("orangered", Colors.OrangeRed);
			AddColor("orchid", Colors.Orchid);
			AddColor("palegoldenrod", Colors.PaleGoldenrod);
			AddColor("palegreen", Colors.PaleGreen);
			AddColor("paleturquoise", Colors.PaleTurquoise);
			AddColor("palevioletred", Colors.PaleVioletRed);
			AddColor("papayawhip", Colors.PapayaWhip);
			AddColor("peachpuff", Colors.PeachPuff);
			AddColor("peru", Colors.Peru);
			AddColor("pink", Colors.Pink);
			AddColor("plum", Colors.Plum);
			AddColor("powderblue", Colors.PowderBlue);
			AddColor("purple", Colors.Purple);
			AddColor("red", Colors.Red);
			AddColor("rosybrown", Colors.RosyBrown);
			AddColor("royalblue", Colors.RoyalBlue);
			AddColor("saddlebrown", Colors.SaddleBrown);
			AddColor("salmon", Colors.Salmon);
			AddColor("sandybrown", Colors.SandyBrown);
			AddColor("seagreen", Colors.SeaGreen);
			AddColor("seashell", Colors.SeaShell);
			AddColor("sienna", Colors.Sienna);
			AddColor("silver", Colors.Silver);
			AddColor("skyblue", Colors.SkyBlue);
			AddColor("slateblue", Colors.SlateBlue);
			AddColor("slategray", Colors.SlateGray);
			AddColor("snow", Colors.Snow);
			AddColor("springgreen", Colors.SpringGreen);
			AddColor("steelblue", Colors.SteelBlue);
			AddColor("tan", Colors.Tan);
			AddColor("teal", Colors.Teal);
			AddColor("thistle", Colors.Thistle);
			AddColor("tomato", Colors.Tomato);
			AddColor("transparent", Colors.Transparent);
			AddColor("turquoise", Colors.Turquoise);
			AddColor("violet", Colors.Violet);
			AddColor("wheat", Colors.Wheat);
			AddColor("white", Colors.White);
			AddColor("whitesmoke", Colors.WhiteSmoke);
			AddColor("yellow", Colors.Yellow);
			AddColor("yellowgreen", Colors.YellowGreen);

			return namedColors;
		}

		// this algorithm was taken from Roslyn when it creates a string jump table, modified to use ToLowerInvariant
		// https://github.com/dotnet/roslyn/blob/6920e1e044dcadbc53e0af7d1582190fdcb88ec6/src/Compilers/CSharp/Portable/Compiler/MethodBodySynthesizer.Lowered.cs#L23
		static uint ComputeIgnoreCaseStringHash(ReadOnlySpan<char> s)
		{
			uint hashCode = 2166136261u;
			for (int i = 0; i < s.Length; i++)
			{
				hashCode = unchecked((char.ToLowerInvariant(s[i]) ^ hashCode) * 16777619);
			}
			return hashCode;
		}

		static bool TryParseFourColorRanges(
			ReadOnlySpan<char> value,
			out ReadOnlySpan<char> quad0,
			out ReadOnlySpan<char> quad1,
			out ReadOnlySpan<char> quad2,
			out ReadOnlySpan<char> quad3)
		{
			var op = value.IndexOf('(');
			var cp = value.LastIndexOf(')');
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

		static bool TryParseThreeColorRanges(
			ReadOnlySpan<char> value,
			out ReadOnlySpan<char> triplet0,
			out ReadOnlySpan<char> triplet1,
			out ReadOnlySpan<char> triplet2)
		{
			var op = value.IndexOf('(');
			var cp = value.LastIndexOf(')');
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

		static double ParseColorValue(ReadOnlySpan<char> elem, int maxValue, bool acceptPercent)
		{
			elem = elem.Trim();
			if (!elem.IsEmpty && elem[elem.Length - 1] == '%' && acceptPercent)
			{
				maxValue = 100;
				elem = elem.Slice(0, elem.Length - 1);
			}
			return ParseDouble(elem).Clamp(0, maxValue) / maxValue;
		}

		static double ParseOpacity(ReadOnlySpan<char> elem)
			=> ParseDouble(elem).Clamp(0, 1);

		static double ParseDouble(ReadOnlySpan<char> s) =>
			double.Parse(
#if NETSTANDARD2_0
				s.ToString(),
#else
				s, 
#endif
				NumberStyles.Number, CultureInfo.InvariantCulture);

		static int ParseInt(ReadOnlySpan<char> s) =>
			int.Parse(
#if NETSTANDARD2_0
				s.ToString(),
#else
				s,
#endif
				 NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

		public static implicit operator Color(Vector4 color) => new Color(color);
	}
}
