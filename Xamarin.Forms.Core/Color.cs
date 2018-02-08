using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[DebuggerDisplay("R={R}, G={G}, B={B}, A={A}, Hue={Hue}, Saturation={Saturation}, Luminosity={Luminosity}")]
	[TypeConverter(typeof(ColorTypeConverter))]
	public struct Color
	{
		readonly Mode _mode;

		enum Mode
		{
			Default,
			Rgb,
			Hsl
		}

		public static Color Default
		{
			get { return new Color(-1d, -1d, -1d, -1d, Mode.Default); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsDefault
		{
			get { return _mode == Mode.Default; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetAccent(Color value) => Accent = value;
		public static Color Accent { get; internal set; }

		readonly float _a;

		public double A
		{
			get { return _a; }
		}

		readonly float _r;

		public double R
		{
			get { return _r; }
		}

		readonly float _g;

		public double G
		{
			get { return _g; }
		}

		readonly float _b;

		public double B
		{
			get { return _b; }
		}

		readonly float _hue;

		public double Hue
		{
			get { return _hue; }
		}

		readonly float _saturation;

		public double Saturation
		{
			get { return _saturation; }
		}

		readonly float _luminosity;

		public double Luminosity
		{
			get { return _luminosity; }
		}

		public Color(double r, double g, double b, double a) : this(r, g, b, a, Mode.Rgb)
		{
		}

		Color(double w, double x, double y, double z, Mode mode)
		{
			_mode = mode;
			switch (mode)
			{
				default:
				case Mode.Default:
					_r = _g = _b = _a = -1;
					_hue = _saturation = _luminosity = -1;
					break;
				case Mode.Rgb:
					_r = (float)w.Clamp(0, 1);
					_g = (float)x.Clamp(0, 1);
					_b = (float)y.Clamp(0, 1);
					_a = (float)z.Clamp(0, 1);
					ConvertToHsl(_r, _g, _b, mode, out _hue, out _saturation, out _luminosity);
					break;
				case Mode.Hsl:
					_hue = (float)w.Clamp(0, 1);
					_saturation = (float)x.Clamp(0, 1);
					_luminosity = (float)y.Clamp(0, 1);
					_a = (float)z.Clamp(0, 1);
					ConvertToRgb(_hue, _saturation, _luminosity, mode, out _r, out _g, out _b);
					break;
			}
		}

		public Color(double r, double g, double b) : this(r, g, b, 1)
		{
		}

		public Color(double value) : this(value, value, value, 1)
		{
		}

		public Color MultiplyAlpha(double alpha)
		{
			switch (_mode)
			{
				default:
				case Mode.Default:
					throw new InvalidOperationException("Invalid on Color.Default");
				case Mode.Rgb:
					return new Color(_r, _g, _b, _a * alpha, Mode.Rgb);
				case Mode.Hsl:
					return new Color(_hue, _saturation, _luminosity, _a * alpha, Mode.Hsl);
			}
		}

		public Color AddLuminosity(double delta)
		{
			if (_mode == Mode.Default)
				throw new InvalidOperationException("Invalid on Color.Default");

			return new Color(_hue, _saturation, _luminosity + delta, _a, Mode.Hsl);
		}

		public Color WithHue(double hue)
		{
			if (_mode == Mode.Default)
				throw new InvalidOperationException("Invalid on Color.Default");
			return new Color(hue, _saturation, _luminosity, _a, Mode.Hsl);
		}

		public Color WithSaturation(double saturation)
		{
			if (_mode == Mode.Default)
				throw new InvalidOperationException("Invalid on Color.Default");
			return new Color(_hue, saturation, _luminosity, _a, Mode.Hsl);
		}

		public Color WithLuminosity(double luminosity)
		{
			if (_mode == Mode.Default)
				throw new InvalidOperationException("Invalid on Color.Default");
			return new Color(_hue, _saturation, luminosity, _a, Mode.Hsl);
		}

		static void ConvertToRgb(float hue, float saturation, float luminosity, Mode mode, out float r, out float g, out float b)
		{
			if (mode != Mode.Hsl)
				throw new InvalidOperationException();

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

		static void ConvertToHsl(float r, float g, float b, Mode mode, out float h, out float s, out float l)
		{
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

		public static bool operator ==(Color color1, Color color2)
		{
			return EqualsInner(color1, color2);
		}

		public static bool operator !=(Color color1, Color color2)
		{
			return !EqualsInner(color1, color2);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashcode = _r.GetHashCode();
				hashcode = (hashcode * 397) ^ _g.GetHashCode();
				hashcode = (hashcode * 397) ^ _b.GetHashCode();
				hashcode = (hashcode * 397) ^ _a.GetHashCode();
				return hashcode;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is Color)
			{
				return EqualsInner(this, (Color)obj);
			}
			return base.Equals(obj);
		}

		static bool EqualsInner(Color color1, Color color2)
		{
			if (color1._mode == Mode.Default && color2._mode == Mode.Default)
				return true;
			if (color1._mode == Mode.Default || color2._mode == Mode.Default)
				return false;
			if (color1._mode == Mode.Hsl && color2._mode == Mode.Hsl)
				return color1._hue == color2._hue && color1._saturation == color2._saturation && color1._luminosity == color2._luminosity && color1._a == color2._a;
			return color1._r == color2._r && color1._g == color2._g && color1._b == color2._b && color1._a == color2._a;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[Color: A={0}, R={1}, G={2}, B={3}, Hue={4}, Saturation={5}, Luminosity={6}]", A, R, G, B, Hue, Saturation, Luminosity);
		}

		static uint ToHex (char c)
		{
			ushort x = (ushort)c;
			if (x >= '0' && x <= '9')
				return (uint)(x - '0');

			x |= 0x20;
			if (x >= 'a' && x <= 'f')
				return (uint)(x - 'a' + 10);
			return 0;
		}

		static uint ToHexD (char c)
		{
			var j = ToHex (c);
			return (j << 4) | j;
		}

		public static Color FromHex (string hex)
		{
			// Undefined
			if (hex.Length < 3)
				return Default;
			int idx = (hex [0] == '#') ? 1 : 0;

			switch (hex.Length - idx) {
			case 3: //#rgb => ffrrggbb
				var t1 = ToHexD (hex [idx++]);
				var t2 = ToHexD (hex [idx++]);
				var t3 = ToHexD (hex [idx]);

				return FromRgb ((int)t1, (int)t2, (int)t3);

			case 4: //#argb => aarrggbb
				var f1 = ToHexD (hex [idx++]);
				var f2 = ToHexD (hex [idx++]);
				var f3 = ToHexD (hex [idx++]);
				var f4 = ToHexD (hex [idx]);
				return FromRgba ((int)f2, (int)f3, (int)f4, (int)f1);

			case 6: //#rrggbb => ffrrggbb
				return FromRgb ((int)(ToHex (hex [idx++]) << 4 | ToHex (hex [idx++])),
						(int)(ToHex (hex [idx++]) << 4 | ToHex (hex [idx++])),
						(int)(ToHex (hex [idx++]) << 4 | ToHex (hex [idx])));
				
			case 8: //#aarrggbb
				var a1 = ToHex (hex [idx++]) << 4 | ToHex (hex [idx++]);
				return FromRgba ((int)(ToHex (hex [idx++]) << 4 | ToHex (hex [idx++])),
						(int)(ToHex (hex [idx++]) << 4 | ToHex (hex [idx++])),
						(int)(ToHex (hex [idx++]) << 4 | ToHex (hex [idx])),
						(int)a1);
				
			default: //everything else will result in unexpected results
				return Default;
			}
		}

		public static Color FromUint(uint argb)
		{
			return FromRgba((byte)((argb & 0x00ff0000) >> 0x10), (byte)((argb & 0x0000ff00) >> 0x8), (byte)(argb & 0x000000ff), (byte)((argb & 0xff000000) >> 0x18));
		}

		public static Color FromRgba(int r, int g, int b, int a)
		{
			double red = (double)r / 255;
			double green = (double)g / 255;
			double blue = (double)b / 255;
			double alpha = (double)a / 255;
			return new Color(red, green, blue, alpha, Mode.Rgb);
		}

		public static Color FromRgb(int r, int g, int b)
		{
			return FromRgba(r, g, b, 255);
		}

		public static Color FromRgba(double r, double g, double b, double a)
		{
			return new Color(r, g, b, a);
		}

		public static Color FromRgb(double r, double g, double b)
		{
			return new Color(r, g, b, 1d, Mode.Rgb);
		}

		public static Color FromHsla(double h, double s, double l, double a = 1d)
		{
			return new Color(h, s, l, a, Mode.Hsl);
		}
#if !NETSTANDARD1_0
		public static implicit operator System.Drawing.Color(Color color)
		{
			if (color.IsDefault)
				return System.Drawing.Color.Empty;
			return System.Drawing.Color.FromArgb((byte)(color._a * 255), (byte)(color._r * 255), (byte)(color._g * 255), (byte)(color._b * 255));
		}

		public static implicit operator Color(System.Drawing.Color color)
		{
			if (color.IsEmpty)
				return Color.Default;
			return FromRgba(color.R, color.G, color.B, color.A);
		}
#endif
		#region Color Definitions

		// matches colors in WPF's System.Windows.Media.Colors
		public static readonly Color AliceBlue = FromRgb(240, 248, 255);
		public static readonly Color AntiqueWhite = FromRgb(250, 235, 215);
		public static readonly Color Aqua = FromRgb(0, 255, 255);
		public static readonly Color Aquamarine = FromRgb(127, 255, 212);
		public static readonly Color Azure = FromRgb(240, 255, 255);
		public static readonly Color Beige = FromRgb(245, 245, 220);
		public static readonly Color Bisque = FromRgb(255, 228, 196);
		public static readonly Color Black = FromRgb(0, 0, 0);
		public static readonly Color BlanchedAlmond = FromRgb(255, 235, 205);
		public static readonly Color Blue = FromRgb(0, 0, 255);
		public static readonly Color BlueViolet = FromRgb(138, 43, 226);
		public static readonly Color Brown = FromRgb(165, 42, 42);
		public static readonly Color BurlyWood = FromRgb(222, 184, 135);
		public static readonly Color CadetBlue = FromRgb(95, 158, 160);
		public static readonly Color Chartreuse = FromRgb(127, 255, 0);
		public static readonly Color Chocolate = FromRgb(210, 105, 30);
		public static readonly Color Coral = FromRgb(255, 127, 80);
		public static readonly Color CornflowerBlue = FromRgb(100, 149, 237);
		public static readonly Color Cornsilk = FromRgb(255, 248, 220);
		public static readonly Color Crimson = FromRgb(220, 20, 60);
		public static readonly Color Cyan = FromRgb(0, 255, 255);
		public static readonly Color DarkBlue = FromRgb(0, 0, 139);
		public static readonly Color DarkCyan = FromRgb(0, 139, 139);
		public static readonly Color DarkGoldenrod = FromRgb(184, 134, 11);
		public static readonly Color DarkGray = FromRgb(169, 169, 169);
		public static readonly Color DarkGreen = FromRgb(0, 100, 0);
		public static readonly Color DarkKhaki = FromRgb(189, 183, 107);
		public static readonly Color DarkMagenta = FromRgb(139, 0, 139);
		public static readonly Color DarkOliveGreen = FromRgb(85, 107, 47);
		public static readonly Color DarkOrange = FromRgb(255, 140, 0);
		public static readonly Color DarkOrchid = FromRgb(153, 50, 204);
		public static readonly Color DarkRed = FromRgb(139, 0, 0);
		public static readonly Color DarkSalmon = FromRgb(233, 150, 122);
		public static readonly Color DarkSeaGreen = FromRgb(143, 188, 143);
		public static readonly Color DarkSlateBlue = FromRgb(72, 61, 139);
		public static readonly Color DarkSlateGray = FromRgb(47, 79, 79);
		public static readonly Color DarkTurquoise = FromRgb(0, 206, 209);
		public static readonly Color DarkViolet = FromRgb(148, 0, 211);
		public static readonly Color DeepPink = FromRgb(255, 20, 147);
		public static readonly Color DeepSkyBlue = FromRgb(0, 191, 255);
		public static readonly Color DimGray = FromRgb(105, 105, 105);
		public static readonly Color DodgerBlue = FromRgb(30, 144, 255);
		public static readonly Color Firebrick = FromRgb(178, 34, 34);
		public static readonly Color FloralWhite = FromRgb(255, 250, 240);
		public static readonly Color ForestGreen = FromRgb(34, 139, 34);
		public static readonly Color Fuchsia = FromRgb(255, 0, 255);
		[Obsolete("Fuschia is obsolete as of version 1.3.0. Please use Fuchsia instead.")]
		public static readonly Color Fuschia = FromRgb(255, 0, 255);
		public static readonly Color Gainsboro = FromRgb(220, 220, 220);
		public static readonly Color GhostWhite = FromRgb(248, 248, 255);
		public static readonly Color Gold = FromRgb(255, 215, 0);
		public static readonly Color Goldenrod = FromRgb(218, 165, 32);
		public static readonly Color Gray = FromRgb(128, 128, 128);
		public static readonly Color Green = FromRgb(0, 128, 0);
		public static readonly Color GreenYellow = FromRgb(173, 255, 47);
		public static readonly Color Honeydew = FromRgb(240, 255, 240);
		public static readonly Color HotPink = FromRgb(255, 105, 180);
		public static readonly Color IndianRed = FromRgb(205, 92, 92);
		public static readonly Color Indigo = FromRgb(75, 0, 130);
		public static readonly Color Ivory = FromRgb(255, 255, 240);
		public static readonly Color Khaki = FromRgb(240, 230, 140);
		public static readonly Color Lavender = FromRgb(230, 230, 250);
		public static readonly Color LavenderBlush = FromRgb(255, 240, 245);
		public static readonly Color LawnGreen = FromRgb(124, 252, 0);
		public static readonly Color LemonChiffon = FromRgb(255, 250, 205);
		public static readonly Color LightBlue = FromRgb(173, 216, 230);
		public static readonly Color LightCoral = FromRgb(240, 128, 128);
		public static readonly Color LightCyan = FromRgb(224, 255, 255);
		public static readonly Color LightGoldenrodYellow = FromRgb(250, 250, 210);
		public static readonly Color LightGray = FromRgb(211, 211, 211);
		public static readonly Color LightGreen = FromRgb(144, 238, 144);
		public static readonly Color LightPink = FromRgb(255, 182, 193);
		public static readonly Color LightSalmon = FromRgb(255, 160, 122);
		public static readonly Color LightSeaGreen = FromRgb(32, 178, 170);
		public static readonly Color LightSkyBlue = FromRgb(135, 206, 250);
		public static readonly Color LightSlateGray = FromRgb(119, 136, 153);
		public static readonly Color LightSteelBlue = FromRgb(176, 196, 222);
		public static readonly Color LightYellow = FromRgb(255, 255, 224);
		public static readonly Color Lime = FromRgb(0, 255, 0);
		public static readonly Color LimeGreen = FromRgb(50, 205, 50);
		public static readonly Color Linen = FromRgb(250, 240, 230);
		public static readonly Color Magenta = FromRgb(255, 0, 255);
		public static readonly Color Maroon = FromRgb(128, 0, 0);
		public static readonly Color MediumAquamarine = FromRgb(102, 205, 170);
		public static readonly Color MediumBlue = FromRgb(0, 0, 205);
		public static readonly Color MediumOrchid = FromRgb(186, 85, 211);
		public static readonly Color MediumPurple = FromRgb(147, 112, 219);
		public static readonly Color MediumSeaGreen = FromRgb(60, 179, 113);
		public static readonly Color MediumSlateBlue = FromRgb(123, 104, 238);
		public static readonly Color MediumSpringGreen = FromRgb(0, 250, 154);
		public static readonly Color MediumTurquoise = FromRgb(72, 209, 204);
		public static readonly Color MediumVioletRed = FromRgb(199, 21, 133);
		public static readonly Color MidnightBlue = FromRgb(25, 25, 112);
		public static readonly Color MintCream = FromRgb(245, 255, 250);
		public static readonly Color MistyRose = FromRgb(255, 228, 225);
		public static readonly Color Moccasin = FromRgb(255, 228, 181);
		public static readonly Color NavajoWhite = FromRgb(255, 222, 173);
		public static readonly Color Navy = FromRgb(0, 0, 128);
		public static readonly Color OldLace = FromRgb(253, 245, 230);
		public static readonly Color Olive = FromRgb(128, 128, 0);
		public static readonly Color OliveDrab = FromRgb(107, 142, 35);
		public static readonly Color Orange = FromRgb(255, 165, 0);
		public static readonly Color OrangeRed = FromRgb(255, 69, 0);
		public static readonly Color Orchid = FromRgb(218, 112, 214);
		public static readonly Color PaleGoldenrod = FromRgb(238, 232, 170);
		public static readonly Color PaleGreen = FromRgb(152, 251, 152);
		public static readonly Color PaleTurquoise = FromRgb(175, 238, 238);
		public static readonly Color PaleVioletRed = FromRgb(219, 112, 147);
		public static readonly Color PapayaWhip = FromRgb(255, 239, 213);
		public static readonly Color PeachPuff = FromRgb(255, 218, 185);
		public static readonly Color Peru = FromRgb(205, 133, 63);
		public static readonly Color Pink = FromRgb(255, 192, 203);
		public static readonly Color Plum = FromRgb(221, 160, 221);
		public static readonly Color PowderBlue = FromRgb(176, 224, 230);
		public static readonly Color Purple = FromRgb(128, 0, 128);
		public static readonly Color Red = FromRgb(255, 0, 0);
		public static readonly Color RosyBrown = FromRgb(188, 143, 143);
		public static readonly Color RoyalBlue = FromRgb(65, 105, 225);
		public static readonly Color SaddleBrown = FromRgb(139, 69, 19);
		public static readonly Color Salmon = FromRgb(250, 128, 114);
		public static readonly Color SandyBrown = FromRgb(244, 164, 96);
		public static readonly Color SeaGreen = FromRgb(46, 139, 87);
		public static readonly Color SeaShell = FromRgb(255, 245, 238);
		public static readonly Color Sienna = FromRgb(160, 82, 45);
		public static readonly Color Silver = FromRgb(192, 192, 192);
		public static readonly Color SkyBlue = FromRgb(135, 206, 235);
		public static readonly Color SlateBlue = FromRgb(106, 90, 205);
		public static readonly Color SlateGray = FromRgb(112, 128, 144);
		public static readonly Color Snow = FromRgb(255, 250, 250);
		public static readonly Color SpringGreen = FromRgb(0, 255, 127);
		public static readonly Color SteelBlue = FromRgb(70, 130, 180);
		public static readonly Color Tan = FromRgb(210, 180, 140);
		public static readonly Color Teal = FromRgb(0, 128, 128);
		public static readonly Color Thistle = FromRgb(216, 191, 216);
		public static readonly Color Tomato = FromRgb(255, 99, 71);
		public static readonly Color Transparent = FromRgba(255, 255, 255, 0);
		public static readonly Color Turquoise = FromRgb(64, 224, 208);
		public static readonly Color Violet = FromRgb(238, 130, 238);
		public static readonly Color Wheat = FromRgb(245, 222, 179);
		public static readonly Color White = FromRgb(255, 255, 255);
		public static readonly Color WhiteSmoke = FromRgb(245, 245, 245);
		public static readonly Color Yellow = FromRgb(255, 255, 0);
		public static readonly Color YellowGreen = FromRgb(154, 205, 50);

		#endregion
	}
}
