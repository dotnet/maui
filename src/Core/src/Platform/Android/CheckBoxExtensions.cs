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