using System.Runtime.CompilerServices;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidX.Core.Content;
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

		// Tracks SwitchCompat instances that have had a custom ThumbColor applied, so the
		// platform default thumb tint is only rebuilt when a previously-set custom color is
		// cleared. Default switches that never had a custom ThumbColor keep their native/theme
		// thumb appearance untouched.
		static readonly ConditionalWeakTable<ASwitch, object> _customThumbTintApplied = new();
		static readonly object s_customThumbTintMarker = new();

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

				// Remember that a custom tint was applied so the platform default can be
				// restored if ThumbColor is later cleared to null.
				_customThumbTintApplied.Remove(aSwitch);
				_customThumbTintApplied.Add(aSwitch, s_customThumbTintMarker);
			}
			else if (_customThumbTintApplied.TryGetValue(aSwitch, out _))
			{
				// A custom tint was previously applied; simply clearing it (setting null)
				// strips the native state-based colors and leaves a white/transparent thumb.
				// Rebuild the platform default thumb tint from the current theme's checked,
				// normal, and disabled state colors so the native appearance is restored.
				aSwitch.ThumbTintList = aSwitch.GetDefaultThumbColorStateList();
				_customThumbTintApplied.Remove(aSwitch);
			}
		}

		// Recreates the platform default switch thumb tint per state:
		//   disabled -> switch_thumb_disabled_material_(light|dark) (opaque grey)
		//   checked  -> colorControlActivated
		//   normal   -> colorSwitchThumbNormal
		static ColorStateList? GetDefaultThumbColorStateList(this ASwitch aSwitch)
		{
			var context = aSwitch.Context;
			if (context is null)
			{
				return null;
			}

			var normalColor = context.GetThemeAttrColor(Resource.Attribute.colorSwitchThumbNormal);
			var activatedColor = context.GetThemeAttrColor(Resource.Attribute.colorControlActivated);

			// The native disabled thumb is an opaque grey baked into
			// @color/switch_thumb_disabled_material_(light|dark). It is NOT
			// colorSwitchThumbNormal * disabledAlpha, which would produce a nearly
			// transparent thumb on a light background.
			var isDarkTheme = (context.Resources?.Configuration?.UiMode & UiMode.NightMask) == UiMode.NightYes;
			var disabledColorRes = isDarkTheme
				? Resource.Color.switch_thumb_disabled_material_dark
				: Resource.Color.switch_thumb_disabled_material_light;
			var disabledColor = ContextCompat.GetColor(context, disabledColorRes);

			return ColorStateListExtensions.CreateSwitch(disabledColor, activatedColor, normalColor);
		}

		public static Drawable? GetDefaultSwitchTrackDrawable(this ASwitch aSwitch) =>
			aSwitch.TrackDrawable;

		public static Drawable? GetDefaultSwitchThumbDrawable(this ASwitch aSwitch) =>
			aSwitch.ThumbDrawable;
	}
}