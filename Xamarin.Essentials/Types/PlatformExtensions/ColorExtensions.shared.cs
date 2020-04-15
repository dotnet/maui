#if !NETSTANDARD1_0
using System;
using System.Drawing;

namespace Xamarin.Essentials
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
    }
}
#endif
