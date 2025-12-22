using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
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

		// todo make public for net11
		/// <summary>
		/// Safely applies tint to an ImageView's drawable by mutating it first.
		/// This prevents crashes when the drawable is shared across multiple views.
		/// </summary>
		/// <remarks>
		/// Android shares Drawable resources for memory efficiency. Modifying a shared
		/// drawable without calling Mutate() first causes race conditions and crashes.
		/// See: https://developer.android.com/reference/android/graphics/drawable/Drawable#mutate()
		/// </remarks>
		internal static void SafeSetTint(this ImageView? imageView, Graphics.Color color)
		{
			if (imageView?.Drawable is not ADrawable drawable)
				return;

			var safe = drawable.Mutate();
			safe?.SetTint(color.ToInt());
			imageView.SetImageDrawable(safe);
		}

		/// <summary>
		/// Safely applies tint to an ImageView's drawable by mutating it first.
		/// This prevents crashes when the drawable is shared across multiple views.
		/// </summary>
		/// <remarks>
		/// Android shares Drawable resources for memory efficiency. Modifying a shared
		/// drawable without calling Mutate() first causes race conditions and crashes.
		/// See: https://developer.android.com/reference/android/graphics/drawable/Drawable#mutate()
		/// </remarks>
		internal static void SafeSetTint(this ImageView? imageView, AColor color)
		{
			if (imageView?.Drawable is not ADrawable drawable)
				return;

			var safe = drawable.Mutate();
			safe?.SetTint(color);
			imageView.SetImageDrawable(safe);
		}

		// todo make public for net11
		/// <summary>
		/// Safely applies tint to a drawable by mutating it first.
		/// This prevents crashes when the drawable is shared across multiple views.
		/// </summary>
		/// <remarks>
		/// Android shares Drawable resources for memory efficiency. Modifying a shared
		/// drawable without calling Mutate() first causes race conditions and crashes.
		/// See: https://developer.android.com/reference/android/graphics/drawable/Drawable#mutate()
		/// </remarks>
		/// <returns>The mutated drawable with tint applied, or the original drawable if mutation failed.</returns>
		internal static ADrawable? SafeSetTint(this ADrawable? drawable, Graphics.Color color)
		{
			if (drawable is null)
				return null;

			var safe = drawable.Mutate();
			safe?.SetTint(color.ToInt());
			return safe ?? drawable;
		}

		// todo make public for net11
		/// <summary>
		/// Safely applies tint to a drawable by mutating it first.
		/// This prevents crashes when the drawable is shared across multiple views.
		/// </summary>
		/// <remarks>
		/// Android shares Drawable resources for memory efficiency. Modifying a shared
		/// drawable without calling Mutate() first causes race conditions and crashes.
		/// See: https://developer.android.com/reference/android/graphics/drawable/Drawable#mutate()
		/// </remarks>
		/// <returns>The mutated drawable with tint applied, or the original drawable if mutation failed.</returns>
		internal static ADrawable? SafeSetTint(this ADrawable? drawable, AColor color)
		{
			if (drawable is null)
				return null;

			var safe = drawable.Mutate();
			safe?.SetTint(color);
			return safe ?? drawable;
		}

		// todo make public for net11
		/// <summary>
		/// Safely applies color filter to a drawable by mutating it first.
		/// This prevents crashes when the drawable is shared across multiple views.
		/// </summary>
		/// <remarks>
		/// Android shares Drawable resources for memory efficiency. Modifying a shared
		/// drawable without calling Mutate() first causes race conditions and crashes.
		/// See: https://developer.android.com/reference/android/graphics/drawable/Drawable#mutate()
		/// </remarks>
		/// <returns>The mutated drawable with color filter applied, or the original drawable if mutation failed.</returns>
		internal static ADrawable? SafeSetColorFilter(this ADrawable? drawable, AColor color, FilterMode mode)
		{
			if (drawable is null)
				return null;

			var safe = drawable.Mutate();
			safe?.SetColorFilter(color, mode);
			return safe ?? drawable;
		}

		// todo make public for net11
		/// <summary>
		/// Safely clears color filter from a drawable by mutating it first.
		/// This prevents crashes when the drawable is shared across multiple views.
		/// </summary>
		/// <remarks>
		/// Android shares Drawable resources for memory efficiency. Modifying a shared
		/// drawable without calling Mutate() first causes race conditions and crashes.
		/// See: https://developer.android.com/reference/android/graphics/drawable/Drawable#mutate()
		/// </remarks>
		/// <returns>The mutated drawable with color filter cleared, or the original drawable if mutation failed.</returns>
		internal static ADrawable? SafeClearColorFilter(this ADrawable? drawable)
		{
			if (drawable is null)
				return null;

			var safe = drawable.Mutate();
			safe?.ClearColorFilter();
			return safe ?? drawable;
		}
	}
}