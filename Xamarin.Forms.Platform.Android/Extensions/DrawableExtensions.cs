using System;
using ADrawable = Android.Graphics.Drawables.Drawable;
using AColorFilter = Android.Graphics.ColorFilter;
using AColor = Android.Graphics.Color;
#if __ANDROID_29__
using ADrawableCompat = AndroidX.Core.Graphics.Drawable.DrawableCompat;
#else
using ADrawableCompat = Android.Support.V4.Graphics.Drawable.DrawableCompat;
#endif
using Android.Graphics;

namespace Xamarin.Forms.Platform.Android
{
	enum FilterMode
	{
		SrcIn,
		Multiply,
		SrcAtop
	}

	internal static class DrawableExtensions
	{

#if __ANDROID_29__
		public static BlendMode GetFilterMode(FilterMode mode)
		{
			switch (mode)
			{
				case FilterMode.SrcIn:
					return BlendMode.SrcIn;
				case FilterMode.Multiply:
					return BlendMode.Multiply;
				case FilterMode.SrcAtop:
					return BlendMode.SrcAtop;
			}

			throw new Exception("Invalid Mode");
		}

#else
		[Obsolete]
		static PorterDuff.Mode GetFilterMode(FilterMode mode)
		{
			return GetFilterModePre29(mode);
		}
#endif

		[Obsolete]
		static PorterDuff.Mode GetFilterModePre29(FilterMode mode)
		{
			switch (mode)
			{
				case FilterMode.SrcIn:
					return PorterDuff.Mode.SrcIn;
				case FilterMode.Multiply:
					return PorterDuff.Mode.Multiply;
				case FilterMode.SrcAtop:
					return PorterDuff.Mode.SrcAtop;
			}

			throw new Exception("Invalid Mode");
		}

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


		public static void SetColorFilter(this ADrawable drawable, Color color, AColorFilter defaultColorFilter, FilterMode mode)
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

		public static void SetColorFilter(this ADrawable drawable, Color color, FilterMode mode)
		{
			drawable.SetColorFilter(color.ToAndroid(), mode);
		}

		public static void SetColorFilter(this ADrawable drawable, AColor color, FilterMode mode)
		{
#if __ANDROID_29__
			if(Forms.Is29OrNewer)
				drawable.SetColorFilter(new BlendModeColorFilter(color, GetFilterMode(mode)));
			else
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
				drawable.SetColorFilter(color, GetFilterModePre29(mode));
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#else
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			drawable.SetColorFilter(color, GetFilterMode(mode));
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#endif
		}

	}
}
