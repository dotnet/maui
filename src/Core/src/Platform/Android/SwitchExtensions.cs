using System.Runtime.CompilerServices;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidX.Core.Content;
using AColor = Android.Graphics.Color;
using AAttribute = Android.Resource.Attribute;
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
		static readonly int[] s_enabledChecked = [AAttribute.StateEnabled, AAttribute.StateChecked];
		static readonly int[] s_enabledUnchecked = [AAttribute.StateEnabled];
		static readonly int[] s_disabledChecked = [AAttribute.StateChecked];
		static readonly int[] s_disabledUnchecked = [];

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

		public static void UpdateTrackColor(this MSwitch materialSwitch, ISwitch view)
		{
			// Cache the original theme tint before first modification 
			// so it can be restored when TrackColor is cleared.
			if (!_defaultTrackTintCache.TryGetValue(materialSwitch, out var defaultTrackTintList))
			{
				var themeTint = materialSwitch.TrackTintList;
				if (themeTint is not null)
				{
					_defaultTrackTintCache.Add(materialSwitch, themeTint);
					defaultTrackTintList = themeTint;
				}
			}

			var trackColor = view.TrackColor;
			if (defaultTrackTintList is null)
			{
				materialSwitch.TrackTintList = trackColor?.ToDefaultColorStateList();
				return;
			}

			var currentTint = materialSwitch.TrackTintList ?? defaultTrackTintList;
			var isOn = view.IsOn;
			var customTint = trackColor?.ToPlatform().ToArgb();
			var fallback = new AColor(defaultTrackTintList.DefaultColor);
			var defaultOff = defaultTrackTintList.GetColorForState(s_enabledUnchecked, fallback);
			var currentOff = currentTint.GetColorForState(s_enabledUnchecked, new AColor(defaultOff));
			var defaultDisabledOff = defaultTrackTintList.GetColorForState(s_disabledUnchecked, fallback);
			if (customTint is not { } color)
			{
				var currentDisabledOff = currentTint.GetColorForState(s_disabledUnchecked, new AColor(defaultDisabledOff));
				// Keep the checked endpoint while MaterialSwitch animates to the default off tint.
				if (isOn || currentOff != defaultOff || currentDisabledOff != defaultDisabledOff)
					materialSwitch.TrackTintList = defaultTrackTintList;
				return;
			}

			var currentOn = currentTint.GetColorForState(s_enabledChecked, fallback);
			var defaultDisabledOn = defaultTrackTintList.GetColorForState(s_disabledChecked, fallback);
			materialSwitch.TrackTintList = ColorStateListExtensions.CreateSwitchTrack(
				isOn ? color : currentOn,
				isOn ? currentOff : color,
				defaultDisabledOn,
				defaultDisabledOff);
		}

		public static void UpdateThumbColor(this MSwitch materialSwitch, ISwitch view)
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