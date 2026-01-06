using System.Runtime.CompilerServices;
using Android.Content.Res;
using Android.Graphics;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Platform
{
	public static class CheckBoxExtensions
	{
		// Store the original theme button tint per checkbox instance to support:
		// - Per-Activity theming (different Activities can have different themes)
		// - Theme switching at runtime (dark mode, Material2/3 toggle)
		// - Thread safety (no shared mutable state)
		static readonly ConditionalWeakTable<AppCompatCheckBox, ColorStateList> _defaultButtonTintCache = new();

		public static void UpdateBackground(this AppCompatCheckBox platformCheckBox, ICheckBox check)
		{
			var paint = check.Background;

			if (paint.IsNullOrEmpty())
				platformCheckBox.SetBackgroundColor(AColor.Transparent);
			else
				platformCheckBox.UpdateBackground((IView)check);
		}

		public static void UpdateIsChecked(this AppCompatCheckBox platformCheckBox, ICheckBox check)
		{
			platformCheckBox.Checked = check.IsChecked;
		}

		public static void UpdateForeground(this AppCompatCheckBox platformCheckBox, ICheckBox check)
		{
			var mode = PorterDuff.Mode.SrcIn;

			CompoundButtonCompat.SetButtonTintList(platformCheckBox, platformCheckBox.GetColorStateList(check));
			CompoundButtonCompat.SetButtonTintMode(platformCheckBox, mode);
		}

		internal static ColorStateList GetColorStateList(this AppCompatCheckBox platformCheckBox, ICheckBox check)
		{
			// Cache the original theme button tint for this checkbox instance before we modify it.
			// This must happen before SetButtonTintList is called, as that will overwrite ButtonTintList.
			if (!_defaultButtonTintCache.TryGetValue(platformCheckBox, out var defaultButtonTintList))
			{
				var currentTint = platformCheckBox.ButtonTintList;
				if (currentTint is not null)
				{
					_defaultButtonTintCache.Add(platformCheckBox, currentTint);
					defaultButtonTintList = currentTint;
				}
			}

			// For the moment, we're only supporting solid color Paint for the Android Checkbox
			if (check.Foreground is SolidPaint solid)
			{
				var color = solid.Color;
				AColor tintColor = color.ToPlatform();
				return ColorStateListExtensions.CreateCheckBox(tintColor);
			}

			if (RuntimeFeature.IsMaterial3Enabled)
			{
				// Material 3: Use the original theme's buttonTint
				if (defaultButtonTintList is not null)
				{
					return defaultButtonTintList;
				}
			}

			// Material 2: Use accent color
			Graphics.Color accent = platformCheckBox.Context?.GetAccentColor() ?? Graphics.Color.FromArgb("#ff33b5e5");
			AColor tintColor2 = accent.ToPlatform();
			return ColorStateListExtensions.CreateCheckBox(tintColor2);
		}
	}
}
