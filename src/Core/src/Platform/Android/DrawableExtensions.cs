using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using AColor = Android.Graphics.Color;
using AColorFilter = Android.Graphics.ColorFilter;
using ADrawable = Android.Graphics.Drawables.Drawable;
using ADrawableCompat = AndroidX.Core.Graphics.Drawable.DrawableCompat;

namespace Microsoft.Maui.Platform
{
	public static class DrawableExtensions
	{
		[System.Runtime.Versioning.SupportedOSPlatform("android29.0")]
		public static BlendMode? GetFilterMode(FilterMode mode)
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

		public static AColorFilter? GetColorFilter(this ADrawable drawable)
		{
			if (drawable == null)
				return null;

			return ADrawableCompat.GetColorFilter(drawable);
		}

		public static void SetColorFilter(this ADrawable drawable, AColorFilter? colorFilter)
		{
			if (drawable == null)
				return;

			if (colorFilter == null)
				ADrawableCompat.ClearColorFilter(drawable);
			else
				drawable.SetColorFilter(colorFilter);
		}

		public static void SetColorFilter(this ADrawable drawable, Graphics.Color color, FilterMode mode)
		{
			if (drawable == null)
				return;

			if (color != null)
				drawable.SetColorFilter(color.ToPlatform(), mode);
		}

		public static void SetColorFilter(this ADrawable drawable, AColor color, FilterMode mode)
		{
			if (drawable is not null)
				PlatformInterop.SetColorFilter(drawable, color, (int)mode);
		}

		internal static IAnimatable? AsAnimatable(this ADrawable? drawable)
		{
			if (drawable is null)
				return null;

			if (drawable is IAnimatable animatable)
				return animatable;

			if (PlatformInterop.GetAnimatable(drawable) is IAnimatable javaAnimatable)
				return javaAnimatable;

			return null;
		}
	}
}