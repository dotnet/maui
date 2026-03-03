using System.Runtime.CompilerServices;
using Android.Content.Res;
using Android.Graphics.Drawables;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using MSwitch = Google.Android.Material.MaterialSwitch.MaterialSwitch;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		// Store the original theme tint per MaterialSwitch instance to support:
		// Per-Activity theming (different Activities can have different themes)
		// Theme switching at runtime (dark mode toggle)
		// Thread safety (no shared mutable state)
		static readonly ConditionalWeakTable<MSwitch, ColorStateList> _defaultTrackTintCache = new();
		static readonly ConditionalWeakTable<MSwitch, ColorStateList> _defaultThumbTintCache = new();

		public static void UpdateIsOn(this ASwitch aSwitch, ISwitch view)
		{
			aSwitch.Checked = view.IsOn;
		}

		// TODO: material3 - make it public in .net 11
		internal static void UpdateIsOn(this MSwitch materialSwitch, ISwitch view)
		{
			materialSwitch.Checked = view.IsOn;
		}

		public static void UpdateTrackColor(this ASwitch aSwitch, ISwitch view)
		{
			var trackColor = view.TrackColor;

			if (trackColor is not null)
			{
				aSwitch.TrackDrawable?.SetColorFilter(trackColor, FilterMode.SrcAtop);
			}
			else
			{
				aSwitch.TrackDrawable?.ClearColorFilter();
			}
		}

		// TODO: material3 - make it public in .net 11
		internal static void UpdateTrackColor(this MSwitch materialSwitch, ISwitch view)
		{
			var trackColor = view.TrackColor;

			// GetValue is thread-safe: atomically stores the original theme tint on first access
			// so that resetting TrackColor always restores the real theme value.
			// The null-coalescing fallback satisfies ConditionalWeakTable's no-null-value constraint;
			// in practice MaterialSwitch always has a theme-supplied tint, so the fallback is never reached.
			var defaultTrackTintList = _defaultTrackTintCache.GetValue(
				materialSwitch,
				static m => m.TrackTintList ?? ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent));

			if (trackColor is not null)
			{
				materialSwitch.TrackTintList = ColorStateList.ValueOf(trackColor.ToPlatform());
			}
			else
			{
				materialSwitch.TrackTintList = defaultTrackTintList;
			}
		}

		internal static void UpdateThumbColor(this MSwitch materialSwitch, ISwitch view)
		{
			var thumbColor = view.ThumbColor;

			// GetValue is thread-safe: atomically stores the original theme tint on first access
			// so that resetting ThumbColor always restores the real theme value.
			// The null-coalescing fallback satisfies ConditionalWeakTable's no-null-value constraint;
			// in practice MaterialSwitch always has a theme-supplied tint, so the fallback is never reached.
			var defaultThumbTintList = _defaultThumbTintCache.GetValue(
				materialSwitch,
				static m => m.ThumbTintList ?? ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent));

			if (thumbColor is not null)
			{
				materialSwitch.ThumbTintList = ColorStateList.ValueOf(thumbColor.ToPlatform());
			}
			else
			{
				materialSwitch.ThumbTintList = defaultThumbTintList;
			}
		}

		public static void UpdateThumbColor(this ASwitch aSwitch, ISwitch view)
		{
			var thumbColor = view.ThumbColor;

			if (thumbColor is not null)
			{
				aSwitch.ThumbDrawable?.SetColorFilter(thumbColor, FilterMode.SrcAtop);
			}
		}

		public static Drawable? GetDefaultSwitchTrackDrawable(this ASwitch aSwitch) =>
			aSwitch.TrackDrawable;

		public static Drawable? GetDefaultSwitchThumbDrawable(this ASwitch aSwitch) =>
			aSwitch.ThumbDrawable;
	}
}