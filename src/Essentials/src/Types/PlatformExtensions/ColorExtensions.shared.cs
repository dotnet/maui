#if !NETSTANDARD1_0
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Essentials
{
    public static partial class ColorExtensions
    {
        public static Color MultiplyAlpha(this Color color, float percentage)
        {
            return Color.FromArgb((int)(color.A * percentage), color.R, color.G, color.B);
        }

        public static Color AddLuminosity(this Color color, float delta)
        {
            ColorConverters.ConvertToHsl(color.R / 255f, color.G / 255f, color.B / 255f, out var h, out var s, out var l);
            var newL = l + delta;
            ColorConverters.ConvertToRgb(h, s, newL, out var r, out var g, out var b);
            return Color.FromArgb(color.A, r, g, b);
        }

        public static Color WithHue(this Color color, float hue)
        {
            ColorConverters.ConvertToHsl(color.R / 255f, color.G / 255f, color.B / 255f, out var h, out var s, out var l);
            ColorConverters.ConvertToRgb(hue / 360f, s, l, out var r, out var g, out var b);
            return Color.FromArgb(color.A, r, g, b);
        }

        public static Color WithSaturation(this Color color, float saturation)
        {
            ColorConverters.ConvertToHsl(color.R / 255f, color.G / 255f, color.B / 255f, out var h, out var s, out var l);
            ColorConverters.ConvertToRgb(h, saturation / 100f, l, out var r, out var g, out var b);
            return Color.FromArgb(color.A, r, g, b);
        }

        public static Color WithAlpha(this Color color, int alpha) =>
            Color.FromArgb(alpha, color.R, color.G, color.B);

        public static Color WithLuminosity(this Color color, float luminosity)
        {
            ColorConverters.ConvertToHsl(color.R / 255f, color.G / 255f, color.B / 255f, out var h, out var s, out var l);
            ColorConverters.ConvertToRgb(h, s, luminosity / 100f, out var r, out var g, out var b);
            return Color.FromArgb(color.A, r, g, b);
        }

        public static uint ToUInt(this Color color) =>
            (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));

        public static int ToInt(this Color color) =>
            (color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0);

        public static Color GetComplementary(this Color color)
        {
            // Divide RGB by 255 as ConvertToHsl expects a value between 0 & 1.
            ColorConverters.ConvertToHsl(color.R / 255f, color.G / 255f, color.B / 255f, out var h, out var s, out var l);

            // Multiply by 360 as `ConvertToHsl` specifies it as a value between 0 and 1.
            h *= 360;

            // Add 180 (degrees) to get to the other side of the circle.
            h += 180;

            // Ensure still within the bounds of a circle.
            h %= 360;

            // multiply by 100 as `ConvertToHsl` specifies them as values between 0 and 1.
            return ColorConverters.FromHsl(h, s * 100, l * 100);
        }

        public static (double h, double s, double v) ToHsv(this Color color)
        {
            double h = 0, s;
            var rHSV = (double)color.R / 255;
            var gHSV = (double)color.G / 255;
            var bHSV = (double)color.B / 255;

            var v = Math.Max(Math.Max(rHSV, gHSV), bHSV);
            var cMin = Math.Min(Math.Min(rHSV, gHSV), bHSV);
            var delta = v - cMin;

            if (v == 0)
                s = 0;
            else
                s = delta / v;

            if (delta == 0)
            {
                h = 0;
            }
            else
            {
                if (rHSV == v)
                    h = (gHSV - bHSV) / delta;
                else if (gHSV == v)
                    h = 2 + ((bHSV - rHSV) / delta);
                else if (bHSV == v)
                    h = 4 + ((rHSV - gHSV) / delta);

                h *= 60;

                if (h < 0.0)
                    h += 360;
            }
            s *= 100;
            v *= 100;
            return (h, s, v);
        }

        public static Color FromHsva(double h, double s, double v, double a)
        {
            h /= 360d;
            s /= 100d;
            v /= 100d;

            h = h.Clamp(0, 1);
            s = s.Clamp(0, 1);
            v = v.Clamp(0, 1);
            var range = (int)Math.Floor(h * 6) % 6;
            var f = (h * 6) - Math.Floor(h * 6);
            var p = v * (1 - s);
            var q = v * (1 - (f * s));
            var t = v * (1 - (s * (1 - f)));

            switch (range)
            {
                case 0:
                    return ToRgba(v, t, p, a);
                case 1:
                    return ToRgba(q, v, p, a);
                case 2:
                    return ToRgba(p, v, t, a);
                case 3:
                    return ToRgba(p, q, v, a);
                case 4:
                    return ToRgba(t, p, v, a);
            }
            return ToRgba(v, p, q, a);

            static Color ToRgba(double r, double g, double b, double a)
            {
                r = r.Clamp(0, 1);
                g = g.Clamp(0, 1);
                b = b.Clamp(0, 1);

                return Color.FromArgb((int)a, (int)(r * 255), (int)(g * 255), (int)(b * 255));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double Clamp(this double self, double min, double max)
        {
            if (max < min)
            {
                return max;
            }
            else if (self < min)
            {
                return min;
            }
            else if (self > max)
            {
                return max;
            }

            return self;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Clamp(this int self, int min, int max)
        {
            if (max < min)
            {
                return max;
            }
            else if (self < min)
            {
                return min;
            }
            else if (self > max)
            {
                return max;
            }

            return self;
        }
    }
}
#endif
