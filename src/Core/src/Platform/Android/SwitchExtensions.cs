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
			// Cache the original theme tint before first modification 
			// so it can be restored when TrackColor is cleared.
			if (!_defaultTrackTintCache.TryGetValue(materialSwitch, out var defaultTrackTintList))
			{
				var currentTint = materialSwitch.TrackTintList;
				if (currentTint is not null)
				{
					_defaultTrackTintCache.Add(materialSwitch, currentTint);
					defaultTrackTintList = currentTint;
				}
			}
			
			var trackColor = view.TrackColor;

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
			// Cache the original theme tint before first modification 
			// so it can be restored when ThumbColor is cleared.
			if (!_defaultThumbTintCache.TryGetValue(materialSwitch, out var defaultThumbTintList))
			{
				var currentTint = materialSwitch.ThumbTintList;
				if (currentTint is not null)
				{
					_defaultThumbTintCache.Add(materialSwitch, currentTint);
					defaultThumbTintList = currentTint;
				}
			}

			var thumbColor = view.ThumbColor;
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
				// Use ThumbTintList instead of SetColorFilter to preserve the thumb shadow
				// SetColorFilter flattens the drawable and removes the shadow effect
				aSwitch.ThumbTintList = ColorStateListExtensions.CreateDefault(thumbColor.ToPlatform());
			}
		}

		public static Drawable? GetDefaultSwitchTrackDrawable(this ASwitch aSwitch) =>
			aSwitch.TrackDrawable;

		public static Drawable? GetDefaultSwitchThumbDrawable(this ASwitch aSwitch) =>
			aSwitch.ThumbDrawable;
	}
}