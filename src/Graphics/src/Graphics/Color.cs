using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents an RGBA color with floating-point components in the range of 0.0 to 1.0.
	/// </summary>
	[DebuggerDisplay("Red={Red}, Green={Green}, Blue={Blue}, Alpha={Alpha}")]
	[TypeConverter(typeof(Converters.ColorTypeConverter))]
	public class Color
	{
		/// <summary>
		/// The red component of the color, ranging from 0.0 to 1.0.
		/// </summary>
		public readonly float Red;

		/// <summary>
		/// The green component of the color, ranging from 0.0 to 1.0.
		/// </summary>
		public readonly float Green;

		/// <summary>
		/// The blue component of the color, ranging from 0.0 to 1.0.
		/// </summary>
		public readonly float Blue;

		/// <summary>
		/// The alpha (opacity) component of the color, ranging from 0.0 (transparent) to 1.0 (opaque).
		/// </summary>
		public readonly float Alpha = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="Color"/> class with default values (black).
		/// </summary>
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
			var r = Red.ToString(CultureInfo.InvariantCulture);
			var g = Green.ToString(CultureInfo.InvariantCulture);
			var b = Blue.ToString(CultureInfo.InvariantCulture);
			var a = Alpha.ToString(CultureInfo.InvariantCulture);
			return $"[Color: Red={r}, Green={g}, Blue={b}, Alpha={a}]";
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
				return ToInt() == other.ToInt();

			return base.Equals(obj);
		}

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
			if (Math.Abs(alpha - Alpha) < GeometryUtil.Epsilon)
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
			(float r, float g, float b) = ColorUtils.ConvertHsvToRgb(h, s, v);
			return new Color(r, g, b, a);
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

		public static Color FromRgba(string colorAsHex) => FromRgba(colorAsHex != null ? colorAsHex.AsSpan() : default);

		static Color FromRgba(ReadOnlySpan<char> colorAsHex)
		{
			if (ColorUtils.TryParseRgbaHexFormat(colorAsHex, out float red, out float green, out float blue, out float alpha))
			{
				return new Color(red, green, blue, alpha);
			}

			return FromRgba(0f, 0f, 0f, 1f);
		}

		public static Color FromArgb(string colorAsHex) => FromArgb(colorAsHex != null ? colorAsHex.AsSpan() : default);

		static Color FromArgb(ReadOnlySpan<char> colorAsHex)
		{
			if (ColorUtils.TryParseHex(colorAsHex, out float red, out float green, out float blue, out float alpha))
			{
				return new Color(red, green, blue, alpha);
			}

			return FromRgba(0f, 0f, 0f, 1f);
		}

		public static Color FromHsla(float h, float s, float l, float a = 1)
		{
			(float r, float g, float b) = ColorUtils.ConvertHslToRgb(h, s, l);
			return new Color(r, g, b, a);
		}

		public static Color FromHsla(double h, double s, double l, double a = 1)
		{
			(float r, float g, float b) = ColorUtils.ConvertHslToRgb((float)h, (float)s, (float)l);
			return new Color(r, g, b, (float)a);
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
			if (ColorUtils.TryParse(value, out float red, out float green, out float blue, out float alpha))
			{
				color = new Color(red, green, blue, alpha);
				return true;
			}

			color = GetNamedColor(value);
			return color is not null;
		}

		static Color GetNamedColor(ReadOnlySpan<char> value)
		{
			// the longest built-in Color's name is much lower than this check, so we should not allocate here in a typical usage
			Span<char> loweredValue = value.Length <= 128 ? stackalloc char[value.Length] : new char[value.Length];

			int charsWritten = value.ToLowerInvariant(loweredValue);
			Debug.Assert(charsWritten == value.Length);

			return loweredValue switch
			{
				"default" => default,
				"aliceblue" => Colors.AliceBlue,
				"antiquewhite" => Colors.AntiqueWhite,
				"aqua" => Colors.Aqua,
				"aquamarine" => Colors.Aquamarine,
				"azure" => Colors.Azure,
				"beige" => Colors.Beige,
				"bisque" => Colors.Bisque,
				"black" => Colors.Black,
				"blanchedalmond" => Colors.BlanchedAlmond,
				"blue" => Colors.Blue,
				"blueviolet" => Colors.BlueViolet,
				"brown" => Colors.Brown,
				"burlywood" => Colors.BurlyWood,
				"cadetblue" => Colors.CadetBlue,
				"chartreuse" => Colors.Chartreuse,
				"chocolate" => Colors.Chocolate,
				"coral" => Colors.Coral,
				"cornflowerblue" => Colors.CornflowerBlue,
				"cornsilk" => Colors.Cornsilk,
				"crimson" => Colors.Crimson,
				"cyan" => Colors.Cyan,
				"darkblue" => Colors.DarkBlue,
				"darkcyan" => Colors.DarkCyan,
				"darkgoldenrod" => Colors.DarkGoldenrod,
				"darkgray" => Colors.DarkGray,
				"darkgreen" => Colors.DarkGreen,
				"darkgrey" => Colors.DarkGrey,
				"darkkhaki" => Colors.DarkKhaki,
				"darkmagenta" => Colors.DarkMagenta,
				"darkolivegreen" => Colors.DarkOliveGreen,
				"darkorange" => Colors.DarkOrange,
				"darkorchid" => Colors.DarkOrchid,
				"darkred" => Colors.DarkRed,
				"darksalmon" => Colors.DarkSalmon,
				"darkseagreen" => Colors.DarkSeaGreen,
				"darkslateblue" => Colors.DarkSlateBlue,
				"darkslategray" => Colors.DarkSlateGray,
				"darkslategrey" => Colors.DarkSlateGrey,
				"darkturquoise" => Colors.DarkTurquoise,
				"darkviolet" => Colors.DarkViolet,
				"deeppink" => Colors.DeepPink,
				"deepskyblue" => Colors.DeepSkyBlue,
				"dimgray" => Colors.DimGray,
				"dimgrey" => Colors.DimGrey,
				"dodgerblue" => Colors.DodgerBlue,
				"firebrick" => Colors.Firebrick,
				"floralwhite" => Colors.FloralWhite,
				"forestgreen" => Colors.ForestGreen,
				"fuchsia" => Colors.Fuchsia,
				"gainsboro" => Colors.Gainsboro,
				"ghostwhite" => Colors.GhostWhite,
				"gold" => Colors.Gold,
				"goldenrod" => Colors.Goldenrod,
				"gray" => Colors.Gray,
				"green" => Colors.Green,
				"grey" => Colors.Grey,
				"greenyellow" => Colors.GreenYellow,
				"honeydew" => Colors.Honeydew,
				"hotpink" => Colors.HotPink,
				"indianred" => Colors.IndianRed,
				"indigo" => Colors.Indigo,
				"ivory" => Colors.Ivory,
				"khaki" => Colors.Khaki,
				"lavender" => Colors.Lavender,
				"lavenderblush" => Colors.LavenderBlush,
				"lawngreen" => Colors.LawnGreen,
				"lemonchiffon" => Colors.LemonChiffon,
				"lightblue" => Colors.LightBlue,
				"lightcoral" => Colors.LightCoral,
				"lightcyan" => Colors.LightCyan,
				"lightgoldenrodyellow" => Colors.LightGoldenrodYellow,
				"lightgrey" => Colors.LightGrey,
				"lightgray" => Colors.LightGray,
				"lightgreen" => Colors.LightGreen,
				"lightpink" => Colors.LightPink,
				"lightsalmon" => Colors.LightSalmon,
				"lightseagreen" => Colors.LightSeaGreen,
				"lightskyblue" => Colors.LightSkyBlue,
				"lightslategray" => Colors.LightSlateGray,
				"lightslategrey" => Colors.LightSlateGrey,
				"lightsteelblue" => Colors.LightSteelBlue,
				"lightyellow" => Colors.LightYellow,
				"lime" => Colors.Lime,
				"limegreen" => Colors.LimeGreen,
				"linen" => Colors.Linen,
				"magenta" => Colors.Magenta,
				"maroon" => Colors.Maroon,
				"mediumaquamarine" => Colors.MediumAquamarine,
				"mediumblue" => Colors.MediumBlue,
				"mediumorchid" => Colors.MediumOrchid,
				"mediumpurple" => Colors.MediumPurple,
				"mediumseagreen" => Colors.MediumSeaGreen,
				"mediumslateblue" => Colors.MediumSlateBlue,
				"mediumspringgreen" => Colors.MediumSpringGreen,
				"mediumturquoise" => Colors.MediumTurquoise,
				"mediumvioletred" => Colors.MediumVioletRed,
				"midnightblue" => Colors.MidnightBlue,
				"mintcream" => Colors.MintCream,
				"mistyrose" => Colors.MistyRose,
				"moccasin" => Colors.Moccasin,
				"navajowhite" => Colors.NavajoWhite,
				"navy" => Colors.Navy,
				"oldlace" => Colors.OldLace,
				"olive" => Colors.Olive,
				"olivedrab" => Colors.OliveDrab,
				"orange" => Colors.Orange,
				"orangered" => Colors.OrangeRed,
				"orchid" => Colors.Orchid,
				"palegoldenrod" => Colors.PaleGoldenrod,
				"palegreen" => Colors.PaleGreen,
				"paleturquoise" => Colors.PaleTurquoise,
				"palevioletred" => Colors.PaleVioletRed,
				"papayawhip" => Colors.PapayaWhip,
				"peachpuff" => Colors.PeachPuff,
				"peru" => Colors.Peru,
				"pink" => Colors.Pink,
				"plum" => Colors.Plum,
				"powderblue" => Colors.PowderBlue,
				"purple" => Colors.Purple,
				"red" => Colors.Red,
				"rosybrown" => Colors.RosyBrown,
				"royalblue" => Colors.RoyalBlue,
				"saddlebrown" => Colors.SaddleBrown,
				"salmon" => Colors.Salmon,
				"sandybrown" => Colors.SandyBrown,
				"seagreen" => Colors.SeaGreen,
				"seashell" => Colors.SeaShell,
				"sienna" => Colors.Sienna,
				"silver" => Colors.Silver,
				"skyblue" => Colors.SkyBlue,
				"slateblue" => Colors.SlateBlue,
				"slategray" => Colors.SlateGray,
				"slategrey" => Colors.SlateGrey,
				"snow" => Colors.Snow,
				"springgreen" => Colors.SpringGreen,
				"steelblue" => Colors.SteelBlue,
				"tan" => Colors.Tan,
				"teal" => Colors.Teal,
				"thistle" => Colors.Thistle,
				"tomato" => Colors.Tomato,
				"transparent" => Colors.Transparent,
				"turquoise" => Colors.Turquoise,
				"violet" => Colors.Violet,
				"wheat" => Colors.Wheat,
				"white" => Colors.White,
				"whitesmoke" => Colors.WhiteSmoke,
				"yellow" => Colors.Yellow,
				"yellowgreen" => Colors.YellowGreen,
				_ => null
			};
		}

		public static implicit operator Color(Vector4 color) => new Color(color);
	}
}
