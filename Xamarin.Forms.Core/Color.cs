using System;
using System.Diagnostics;
using System.Globalization;

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

		internal bool IsDefault
		{
			get { return _mode == Mode.Default; }
		}

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

		public static Color FromHex(string hex)
		{
			hex = hex.Replace("#", "");
			switch (hex.Length)
			{
				case 3: //#rgb => ffrrggbb
					hex = string.Format("ff{0}{1}{2}{3}{4}{5}", hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
					break;
				case 4: //#argb => aarrggbb
					hex = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", hex[0], hex[0], hex[1], hex[1], hex[2], hex[2], hex[3], hex[3]);
					break;
				case 6: //#rrggbb => ffrrggbb
					hex = string.Format("ff{0}", hex);
					break;
			}
			return FromUint(Convert.ToUInt32(hex.Replace("#", ""), 16));
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

		#region Color Definitions

		public static readonly Color Transparent = FromRgba(0, 0, 0, 0);
		public static readonly Color Aqua = FromRgb(0, 255, 255);
		public static readonly Color Black = FromRgb(0, 0, 0);
		public static readonly Color Blue = FromRgb(0, 0, 255);
		public static readonly Color Fuchsia = FromRgb(255, 0, 255);
		[Obsolete("Fuschia is obsolete as of version 1.3, please use the correct spelling of Fuchsia")] public static readonly Color Fuschia = FromRgb(255, 0, 255);
		public static readonly Color Gray = FromRgb(128, 128, 128);
		public static readonly Color Green = FromRgb(0, 128, 0);
		public static readonly Color Lime = FromRgb(0, 255, 0);
		public static readonly Color Maroon = FromRgb(128, 0, 0);
		public static readonly Color Navy = FromRgb(0, 0, 128);
		public static readonly Color Olive = FromRgb(128, 128, 0);
		public static readonly Color Orange = FromRgb(255, 165, 0);
		public static readonly Color Purple = FromRgb(128, 0, 128);
		public static readonly Color Pink = FromRgb(255, 102, 255);
		public static readonly Color Red = FromRgb(255, 0, 0);
		public static readonly Color Silver = FromRgb(192, 192, 192);
		public static readonly Color Teal = FromRgb(0, 128, 128);
		public static readonly Color White = FromRgb(255, 255, 255);
		public static readonly Color Yellow = FromRgb(255, 255, 0);

		#endregion
	}
}