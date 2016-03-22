using System;
using System.Diagnostics;
using System.Linq;
using Android.Content;
using Android.Graphics;

namespace Xamarin.Forms.Platform.Android
{
	internal static class CarouselViewExtensions
	{
		internal static int Area(this System.Drawing.Rectangle rectangle)
		{
			return rectangle.Width * rectangle.Height;
		}

		internal static IntVector BoundTranslation(this System.Drawing.Rectangle viewport, IntVector delta, System.Drawing.Rectangle bound)
		{
			// TODO: generalize the math
			Debug.Assert(delta.X == 0 || delta.Y == 0);

			IntVector start = viewport.LeadingCorner(delta);
			IntVector end = start + delta;
			IntVector clampedEnd = end.Clamp(bound);
			IntVector clampedDelta = clampedEnd - start;
			return clampedDelta;
		}

		internal static IntVector Center(this System.Drawing.Rectangle rectangle)
		{
			return (IntVector)rectangle.Location + (IntVector)rectangle.Size / 2;
		}

		internal static IntVector Clamp(this IntVector position, System.Drawing.Rectangle bound)
		{
			return new IntVector(position.X.Clamp(bound.Left, bound.Right), position.Y.Clamp(bound.Top, bound.Bottom));
		}

		internal static IntVector LeadingCorner(this System.Drawing.Rectangle rectangle, IntVector delta)
		{
			return new IntVector(delta.X < 0 ? rectangle.Left : rectangle.Right, delta.Y < 0 ? rectangle.Top : rectangle.Bottom);
		}

		internal static bool LexicographicallyLess(this System.Drawing.Point source, System.Drawing.Point target)
		{
			if (source.X < target.X)
				return true;

			if (source.X > target.X)
				return false;

			return source.Y < target.Y;
		}

		internal static Rect ToAndroidRectangle(this System.Drawing.Rectangle rectangle)
		{
			return new Rect(rectangle.Left, right: rectangle.Right, top: rectangle.Top, bottom: rectangle.Bottom);
		}

		internal static Rectangle ToFormsRectangle(this System.Drawing.Rectangle rectangle, Context context)
		{
			return new Rectangle(context.FromPixels(rectangle.Left), context.FromPixels(rectangle.Top), context.FromPixels(rectangle.Width), context.FromPixels(rectangle.Height));
		}

		internal static int[] ToRange(this Tuple<int, int> startAndCount)
		{
			return Enumerable.Range(startAndCount.Item1, startAndCount.Item2).ToArray();
		}
	}
}