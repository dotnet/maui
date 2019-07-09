using System;
using ADrawable = Android.Graphics.Drawables.Drawable;
using AColorFilter = Android.Graphics.ColorFilter;
using ADrawableCompat = Android.Support.V4.Graphics.Drawable.DrawableCompat;
using Android.Graphics;

namespace Xamarin.Forms.Platform.Android
{
	internal static class DrawableExtensions
	{
		public static AColorFilter GetColorFilter(this ADrawable drawable)
		{
			if (drawable == null)
				return null;

			return ADrawableCompat.GetColorFilter(drawable);
		}

		public static void SetColorFilter(this ADrawable drawable, AColorFilter colorFilter)
		{
			if (drawable == null)
				return;

			if (colorFilter == null)
				ADrawableCompat.ClearColorFilter(drawable);

			drawable.SetColorFilter(colorFilter);
		}

		public static void SetColorFilter(this ADrawable drawable, Color color, AColorFilter defaultColorFilter, PorterDuff.Mode mode)
		{
			if (drawable == null)
				return;

			if (color == Color.Default)
			{
				SetColorFilter(drawable, defaultColorFilter);
				return;
			}

			drawable.SetColorFilter(color.ToAndroid(), mode);
		}
	}
}
