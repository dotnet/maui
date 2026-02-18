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

			// Cache the original theme track tint for this switch instance before we modify it.
			// This must happen before TrackTintList is set, as that will overwrite the original value.
			if (!_defaultTrackTintCache.TryGetValue(materialSwitch, out var defaultTrackTintList))
			{
				if (materialSwitch.TrackTintList is ColorStateList currentTint)
				{
					_defaultTrackTintCache.Add(materialSwitch, currentTint);
					defaultTrackTintList = currentTint;
				}
			}

			if (trackColor is not null)
			{
				materialSwitch.TrackTintList = ColorStateList.ValueOf(trackColor.ToPlatform());
			}
			else
			{
				materialSwitch.TrackTintList = defaultTrackTintList;
			}
		}

		// TODO: material3 - make it public in .net 11
		internal static void UpdateThumbColor(this MSwitch materialSwitch, ISwitch view)
		{
			var thumbColor = view.ThumbColor;

			// Cache the original theme thumb tint for this switch instance before we modify it.
			// This must happen before ThumbTintList is set, as that will overwrite the original value.
			if (!_defaultThumbTintCache.TryGetValue(materialSwitch, out var defaultThumbTintList))
			{
				if (materialSwitch.ThumbTintList is ColorStateList currentTint)
				{
					_defaultThumbTintCache.Add(materialSwitch, currentTint);
					defaultThumbTintList = currentTint;
				}
			}

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