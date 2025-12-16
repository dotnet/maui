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
		static ColorStateList? _defaultButtonTintList;

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
			// For Material 3, preserve the default Material 3 theme colors
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				_defaultButtonTintList ??= platformCheckBox.ButtonTintList;

				if (check.Foreground is SolidPaint solid)
				{
					// Apply custom color only when enabled
					CompoundButtonCompat.SetButtonTintList(platformCheckBox, ColorStateList.ValueOf(solid.Color.ToPlatform()));
				}
				else
				{
					// Restore Material 3 default theme colors
					CompoundButtonCompat.SetButtonTintList(platformCheckBox, _defaultButtonTintList);
				}
			}
			else
			{
				var mode = PorterDuff.Mode.SrcIn;
				CompoundButtonCompat.SetButtonTintList(platformCheckBox, platformCheckBox.GetColorStateList(check));
				CompoundButtonCompat.SetButtonTintMode(platformCheckBox, mode);
			}
		}

		internal static ColorStateList GetColorStateList(this AppCompatCheckBox platformCheckBox, ICheckBox check)
		{
			AColor tintColor;

			// For the moment, we're only supporting solid color Paint for the Android Checkbox
			if (check.Foreground is SolidPaint solid)
			{
				var color = solid.Color;
				tintColor = color.ToPlatform();
			}
			else
			{
				Graphics.Color accent = platformCheckBox.Context?.GetAccentColor() ?? Graphics.Color.FromArgb("#ff33b5e5");
				tintColor = accent.ToPlatform();
			}

			return ColorStateListExtensions.CreateCheckBox(tintColor);
		}
	}
}