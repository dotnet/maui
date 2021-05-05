using System;
using Android.Graphics;
using Microsoft.Maui;
using AColor = Android.Graphics.Color;
using AColorFilter = Android.Graphics.ColorFilter;
using ADrawable = Android.Graphics.Drawables.Drawable;
using ADrawableCompat = AndroidX.Core.Graphics.Drawable.DrawableCompat;

namespace Microsoft.Maui
{
	public static class DrawableExtensions
	{
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

		[Obsolete]
		static PorterDuff.Mode? GetFilterModePre29(FilterMode mode)
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


		public static void SetColorFilter(this ADrawable drawable, Graphics.Color color, FilterMode mode, AColorFilter? defaultColorFilter)
		{
			if (drawable == null)
				return;

			if (color == null)
				SetColorFilter(drawable, defaultColorFilter);
			else
				drawable.SetColorFilter(color.ToNative(), mode);
		}

		public static void SetColorFilter(this ADrawable drawable, Graphics.Color color, FilterMode mode)
		{
			if (drawable == null)
				return;

			drawable.SetColorFilter(color.ToNative(), mode);
		}

		public static void SetColorFilter(this ADrawable drawable, AColor color, FilterMode mode)
		{
			if (drawable == null)
				return;

			if (NativeVersion.Supports(NativeApis.BlendModeColorFilter))
			{
				BlendMode? filterMode29 = GetFilterMode(mode);

				if (filterMode29 != null)
					drawable.SetColorFilter(new BlendModeColorFilter(color, filterMode29));
			}
			else
			{
#pragma warning disable CS0612 // Type or member is obsolete
				PorterDuff.Mode? filterModePre29 = GetFilterModePre29(mode);
#pragma warning restore CS0612 // Type or member is obsolete

				if (filterModePre29 != null)
#pragma warning disable CS0618 // Type or member is obsolete
					drawable.SetColorFilter(color, filterModePre29);
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}
	}
}