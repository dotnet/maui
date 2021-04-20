using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Essentials
{
	public static partial class ColorExtensions
	{
		public static uint ToUInt(this Color color) =>
			(uint)color.ToInt();

		public static int ToInt(this Color color)
		{
			color.ToRgba(out var r, out var g, out var b, out var a);
			return (a << 24) | (r << 16) | (g << 8) | (b << 0);
		}

		public static Color GetComplementary(this Color color)
		{
			// Divide RGB by 255 as ConvertToHsl expects a value between 0 & 1.
			color.ToHsl(out var h, out var s, out var l);

			// Multiply by 360 as `ConvertToHsl` specifies it as a value between 0 and 1.
			h *= 360;

			// Add 180 (degrees) to get to the other side of the circle.
			h += 180;

			// Ensure still within the bounds of a circle.
			h %= 360;

			// multiply by 100 as `ToHsl` specifies them as values between 0 and 1.
			return Color.FromHsla(h, s * 100, l * 100);
		}

		public static void ToRgb(this Color color, out byte r, out byte g, out byte b) =>
			color.ToRgba(out r, out g, out b, out _);

		public static void ToRgba(this Color color, out byte r, out byte g, out byte b, out byte a)
		{
			r = (byte)(color.Red * 255);
			g = (byte)(color.Green * 255);
			b = (byte)(color.Blue * 255);
			a = (byte)(color.Alpha * 255);
		}

		public static void ToHsl(this Color color, out float h, out float s, out float l)
		{
			var r = color.Red;
			var g = color.Green;
			var b = color.Blue;

			var v = Math.Max(r, g);
			v = Math.Max(v, b);

			var m = Math.Min(r, g);
			m = Math.Min(m, b);

			l = (m + v) / 2.0f;
			if (l <= 0.0)
			{
				h = s = l = 0;
				return;
			}
			var vm = v - m;
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

			var r2 = (v - r) / vm;
			var g2 = (v - g) / vm;
			var b2 = (v - b) / vm;

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
