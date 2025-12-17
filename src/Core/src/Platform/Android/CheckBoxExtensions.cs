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
			var mode = PorterDuff.Mode.SrcIn;

			CompoundButtonCompat.SetButtonTintList(platformCheckBox, platformCheckBox.GetColorStateList(check));
			CompoundButtonCompat.SetButtonTintMode(platformCheckBox, mode);
		}

		internal static ColorStateList GetColorStateList(this AppCompatCheckBox platformCheckBox, ICheckBox check)
		{
			// For the moment, we're only supporting solid color Paint for the Android Checkbox
			if (check.Foreground is SolidPaint solid)
			{
				var color = solid.Color;
				AColor tintColor = color.ToPlatform();
				return ColorStateListExtensions.CreateCheckBox(tintColor);
			}
			else
			{
				
				if (RuntimeFeature.IsMaterial3Enabled)
				{
					// Save the default button tint list
					_defaultButtonTintList ??= platformCheckBox.ButtonTintList;

					// Material 3: Use the default theme's buttonTint
					if (_defaultButtonTintList is not null)
					{
						return _defaultButtonTintList;
					}
				}

				// Material 2: Use accent color
				Graphics.Color accent = platformCheckBox.Context?.GetAccentColor() ?? Graphics.Color.FromArgb("#ff33b5e5");
				AColor tintColor = accent.ToPlatform();
				return ColorStateListExtensions.CreateCheckBox(tintColor);
			}
		}
	}
}
