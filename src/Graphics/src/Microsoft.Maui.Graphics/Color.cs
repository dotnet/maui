using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
	[DebuggerDisplay("Red={Red}, Green={Green}, Blue={Blue}, Alpha={Alpha}")]
	public class Color
	{
		public readonly float Red;
		public readonly float Green;
		public readonly float Blue;
		public readonly float Alpha = 1;

		public Color(float gray)
		{
			Red = Green = Blue = gray.Clamp(0,1);
		}

		public Color(float red, float green, float blue)
		{
			Red = red.Clamp(0, 1);
			Green = green.Clamp(0, 1);
			Blue = blue.Clamp(0, 1);
		}

		public Color(float red, float green, float blue, float alpha)
		{
			Red = red.Clamp(0,1);
			Green = green.Clamp(0,1);
			Blue = blue.Clamp(0,1);
			Alpha = alpha.Clamp(0,1);
		}

		public Color(string colorAsHex)
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

			Red = red / 255f;
			Green = green / 255f;
			Blue = blue / 255f;
			Alpha = alpha / 255f;
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
				return Red == other.Red && Green == other.Green && Blue == other.Blue && Alpha == other.Alpha;

			return base.Equals(obj);
		}

		public string ToHex(bool includeAlpha = false)
		{
			if (includeAlpha || Alpha < 1)
				return "#" + ToHex(Red) + ToHex(Green) + ToHex(Blue) + ToHex(Alpha);

			return "#" + ToHex(Red) + ToHex(Green) + ToHex(Blue);
		}

		public Paint AsPaint()
		{
			return new Paint()
			{
				PaintType = PaintType.Solid,
				StartColor = this
			};
		}
		
		public Color WithAlpha(float alpha)
		{
			if (Math.Abs(alpha - Alpha) < Geometry.Epsilon)
				return this;
			
			return new Color(Red,Green,Blue, alpha);
		}

		public Color MultiplyAlpha(float multiplyBy)
		{
			return new Color(Red,Green,Blue, Alpha * multiplyBy);
		}

		public static string ToHex(float r, float g, float b)
		{
			return "#" + ToHex(r) + ToHex(g) + ToHex(b);
		}

		private static string ToHex(float value)
		{
			var intValue = (int)(255f * value);
			var stringValue = intValue.ToString("X");
			if (stringValue.Length == 1)
				return "0" + stringValue;

			return stringValue;
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
			ConvertToHsl(Red,Green,Blue, out var h, out var s, out var l);
			l+= delta;
			l = l.Clamp(0, 1);
			return FromHsla(h,s,l,Alpha);
		}

		public Color WithLuminosity(float luminosity)
		{
			ConvertToHsl(Red, Green, Blue, out var h, out var s, out var l);
			return FromHsla(h, s, luminosity, Alpha);
		}

		public float GetSaturation()
        {
			ConvertToHsl(Red, Green, Blue, out var h, out var s, out var l);
			return s;
		}

		public Color WithSaturation(float saturation)
		{
			ConvertToHsl(Red, Green, Blue, out var h, out var s, out var l);
			return FromHsla(h, saturation, l, Alpha);
		}

		public float GetHue()
		{
			ConvertToHsl(Red, Green, Blue, out var h, out var s, out var l);
			return h;
		}

		public Color WithHue(float hue)
		{
			ConvertToHsl(Red, Green, Blue, out var h, out var s, out var l);
			return FromHsla(hue, s, l, Alpha);
		}

		public static Color FromHex(string hex)
        {
			return new Color(hex);
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
				alpha = int.Parse($"{colorAsHex[3]}{colorAsHex[0]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				red = int.Parse($"{colorAsHex[0]}{colorAsHex[1]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse($"{colorAsHex[1]}{colorAsHex[2]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse($"{colorAsHex[2]}{colorAsHex[3]}", NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}
			else if (colorAsHex.Length == 8)
			{
				//#AARRGGBB
				alpha = int.Parse(colorAsHex.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				red = int.Parse(colorAsHex.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				green = int.Parse(colorAsHex.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				blue = int.Parse(colorAsHex.Substring(6, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			}

			return FromRgba(red, green, blue, alpha);
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

		private static void ConvertToHsl(float r, float g, float b, out float h, out float s, out float l)
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
	}
}