using System;
using Microsoft.Maui.Graphics;
namespace Microsoft.Maui.Animations
{
	public static class AnimationLerpingExtensions
	{
		public static Color Lerp(this Color color, Color endColor, double progress)
		{
			color ??= Colors.Black;
			endColor ??= Colors.Black;
			float Lerp(float start, float end, double progress) => (float)(((end - start) * progress) + start);

			var r = Lerp(color.Red, endColor.Red, progress);
			var b = Lerp(color.Blue, endColor.Blue, progress);
			var g = Lerp(color.Green, endColor.Green, progress);
			var a = Lerp(color.Alpha, endColor.Alpha, progress);
			return new Color(r, g, b, a);
		}

		public static SizeF Lerp(this SizeF start, SizeF end, double progress) =>
			new SizeF(start.Width.Lerp(end.Width, progress), start.Height.Lerp(end.Height, progress));

		public static PointF Lerp(this PointF start, PointF end, double progress) =>
			new PointF(start.X.Lerp(end.X, progress), start.Y.Lerp(end.Y, progress));

		public static RectF Lerp(this RectF start, RectF end, double progress)
			=> new RectF(start.Location.Lerp(end.Location, progress), start.Size.Lerp(end.Size, progress));

		public static Size Lerp(this Size start, Size end, double progress) =>
			new Size(start.Width.Lerp(end.Width, progress), start.Height.Lerp(end.Height, progress));

		public static Point Lerp(this Point start, Point end, double progress) =>
			new Point(start.X.Lerp(end.X, progress), start.Y.Lerp(end.Y, progress));

		public static Rect Lerp(this Rect start, Rect end, double progress)
			=> new Rect(start.Location.Lerp(end.Location, progress), start.Size.Lerp(end.Size, progress));

		public static float Lerp(this float start, float end, double progress) =>
			(float)((end - start) * progress) + start;

		public static double Lerp(this double start, double end, double progress) =>
			((end - start) * progress) + start;

		//IF there is a null, we toggle at the half way. If both values are set, we can lerp
		public static float? Lerp(this float? start, float? end, double progress)
			=> start.HasValue && end.HasValue ? start.Value.Lerp(end.Value, progress) : start.GenericLerp(end, progress);

		public static T GenericLerp<T>(this T start, T end, double progress, double toggleThreshold = .5)
			=> progress < toggleThreshold ? start : end;

		public static Thickness Lerp(this Thickness start, Thickness end, double progress)
			=> new Thickness(
				start.Left.Lerp(end.Left, progress),
				start.Top.Lerp(end.Top, progress),
				start.Right.Lerp(end.Right, progress),
				start.Bottom.Lerp(end.Bottom, progress)
				);
		public static SolidPaint Lerp(this SolidPaint paint, SolidPaint endPaint, double progress)
		{
			var color = paint?.Color ?? Colors.Black;
			var endColor = endPaint?.Color ?? Colors.Black;
			return new SolidPaint(color.Lerp(endColor, progress));
		}
	}
}
