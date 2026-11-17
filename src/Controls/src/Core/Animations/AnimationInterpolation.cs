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
			(short, short) or (byte, byte) or (uint, uint) or (TimeSpan, TimeSpan) => true,
			(Color, Color) => true,
			(Thickness, Thickness) or (CornerRadius, CornerRadius) => true,
			(Point, Point) or (Size, Size) or (Rect, Rect) => true,
			(PointF, PointF) or (SizeF, SizeF) or (RectF, RectF) => true,
			(GridLength a, GridLength b) => a.GridUnitType == b.GridUnitType && a.GridUnitType != GridUnitType.Auto,
			(DoubleCollection a, DoubleCollection b) => a.Count == b.Count,
			(SolidColorBrush { Color: not null }, SolidColorBrush { Color: not null }) => true,
			_ => false,
		};

		/// <summary>
		/// As <see cref="CanInterpolate(object?, object?)"/>, and an automatic (−1) or unbounded size request holds:
		/// it isn't a number to interpolate. Resolving it to a measured size (spec Part C) isn't prototyped.
		/// </summary>
		public static bool CanInterpolate(BindableProperty property, object? from, object? to)
			=> CanInterpolate(from, to) && !(IsSizeRequest(property) && (IsAutomaticSize(from) || IsAutomaticSize(to)));

		static bool IsSizeRequest(BindableProperty property)
			=> property == VisualElement.WidthRequestProperty || property == VisualElement.HeightRequestProperty
			|| property == VisualElement.MinimumWidthRequestProperty || property == VisualElement.MinimumHeightRequestProperty
			|| property == VisualElement.MaximumWidthRequestProperty || property == VisualElement.MaximumHeightRequestProperty;

		static bool IsAutomaticSize(object? value) => value is double size && !(size >= 0 && size < double.PositiveInfinity);

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
				case short a when to is short b:
					return (short)Clamp(Math.Round(a + (b - (double)a) * t, MidpointRounding.AwayFromZero), short.MinValue, short.MaxValue);
				case byte a when to is byte b:
					return (byte)Clamp(Math.Round(a + (b - (double)a) * t, MidpointRounding.AwayFromZero), byte.MinValue, byte.MaxValue);
				case uint a when to is uint b:
					return (uint)Clamp(Math.Round(a + (b - (double)a) * t, MidpointRounding.AwayFromZero), uint.MinValue, uint.MaxValue);
				case TimeSpan a when to is TimeSpan b:
					return TimeSpan.FromTicks((long)Clamp(Math.Round(a.Ticks + (b.Ticks - (double)a.Ticks) * t, MidpointRounding.AwayFromZero), TimeSpan.MinValue.Ticks, TimeSpan.MaxValue.Ticks));
				case PointF a when to is PointF b:
					return new PointF((float)Lerp(a.X, b.X, t), (float)Lerp(a.Y, b.Y, t));
				case SizeF a when to is SizeF b:
					return new SizeF((float)Lerp(a.Width, b.Width, t), (float)Lerp(a.Height, b.Height, t));
				case RectF a when to is RectF b:
					return new RectF((float)Lerp(a.X, b.X, t), (float)Lerp(a.Y, b.Y, t), (float)Lerp(a.Width, b.Width, t), (float)Lerp(a.Height, b.Height, t));
				case GridLength a when to is GridLength b && a.GridUnitType == b.GridUnitType && a.GridUnitType != GridUnitType.Auto:
					// GridLength rejects negative values, which an overshooting easing can produce.
					return new GridLength(Math.Max(0, Lerp(a.Value, b.Value, t)), a.GridUnitType);
				case DoubleCollection a when to is DoubleCollection b && a.Count == b.Count:
					var values = new double[a.Count];
					for (int i = 0; i < values.Length; i++)
						values[i] = Lerp(a[i], b[i], t);
					return new DoubleCollection(values);
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
