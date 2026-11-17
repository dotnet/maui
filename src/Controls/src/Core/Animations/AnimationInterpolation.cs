#nullable enable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Built-in interpolation (spec Part C). Types that can't be interpolated hold: the value changes at
	/// the end of the segment. The legacy, process-global <c>Lerp</c> table is never consulted.
	/// </summary>
	internal static class AnimationInterpolation
	{
		public static bool CanInterpolate(object? from, object? to) => (from, to) switch
		{
			(double, double) or (float, float) or (int, int) or (long, long) => true,
			(Color, Color) => true,
			(Thickness, Thickness) or (CornerRadius, CornerRadius) => true,
			(Point, Point) or (Size, Size) or (Rect, Rect) => true,
			(SolidColorBrush { Color: not null }, SolidColorBrush { Color: not null }) => true,
			_ => false,
		};

		/// <summary>
		/// Interpolates at eased progress <paramref name="t"/>, which may leave [0, 1] for overshooting
		/// easings. <paramref name="t"/> of exactly 1 returns <paramref name="to"/> itself (rule 5).
		/// </summary>
		public static object? Interpolate(object? from, object? to, double t)
		{
			if (t == 1.0)
				return to;

			switch (from)
			{
				case double a when to is double b:
					return a + (b - a) * t;
				case float a when to is float b:
					return (float)(a + (b - a) * t);
				case int a when to is int b:
					return (int)Clamp(Math.Round(a + (b - (double)a) * t, MidpointRounding.AwayFromZero), int.MinValue, int.MaxValue);
				case long a when to is long b:
					return (long)Clamp(Math.Round(a + (b - (double)a) * t, MidpointRounding.AwayFromZero), long.MinValue, long.MaxValue);
				case Color a when to is Color b:
					return Lerp(a, b, t);
				case Thickness a when to is Thickness b:
					return new Thickness(Lerp(a.Left, b.Left, t), Lerp(a.Top, b.Top, t), Lerp(a.Right, b.Right, t), Lerp(a.Bottom, b.Bottom, t));
				case CornerRadius a when to is CornerRadius b:
					return new CornerRadius(Lerp(a.TopLeft, b.TopLeft, t), Lerp(a.TopRight, b.TopRight, t), Lerp(a.BottomLeft, b.BottomLeft, t), Lerp(a.BottomRight, b.BottomRight, t));
				case Point a when to is Point b:
					return new Point(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
				case Size a when to is Size b:
					return new Size(Lerp(a.Width, b.Width, t), Lerp(a.Height, b.Height, t));
				case Rect a when to is Rect b:
					return new Rect(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t), Lerp(a.Width, b.Width, t), Lerp(a.Height, b.Height, t));
				case SolidColorBrush { Color: { } a } when to is SolidColorBrush { Color: { } b }:
					return new SolidColorBrush(Lerp(a, b, t));
			}

			return t < 1.0 ? from : to;
		}

		static double Lerp(double a, double b, double t) => a + (b - a) * t;

		// Premultiplied-alpha sRGB, so Transparent -> White doesn't pass through grey.
		static Color Lerp(Color a, Color b, double t)
		{
			double alpha = Lerp(a.Alpha, b.Alpha, t);
			if (alpha <= 0)
				return new Color(b.Red, b.Green, b.Blue, 0f);

			double red = Lerp(a.Red * a.Alpha, b.Red * b.Alpha, t) / alpha;
			double green = Lerp(a.Green * a.Alpha, b.Green * b.Alpha, t) / alpha;
			double blue = Lerp(a.Blue * a.Alpha, b.Blue * b.Alpha, t) / alpha;

			return new Color((float)Clamp(red, 0, 1), (float)Clamp(green, 0, 1), (float)Clamp(blue, 0, 1), (float)Clamp(alpha, 0, 1));
		}

		static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;
	}
}
