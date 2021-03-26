using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateBackground(this AppCompatCheckBox nativeCheckBox, ICheckBox check)
		{
			var paint = check.Background;

			if (paint.IsNullOrEmpty())
				nativeCheckBox.SetBackgroundColor(AColor.Transparent);
			else
				nativeCheckBox.UpdateBackground((IView)check);
		}

		public static void UpdateIsChecked(this AppCompatCheckBox nativeCheckBox, ICheckBox check)
		{
			nativeCheckBox.Checked = check.IsChecked;
		}

		public static void UpdateColor(this AppCompatCheckBox nativeCheckBox, ICheckBox check)
		{
			// TODO: Delete when implementing the logic to set the system accent color. 
			XColor accent = XColor.FromHex("#ff33b5e5");

			var tintColor = check.Color == XColor.Default ? accent.ToNative() : check.Color.ToNative();

			var tintList = new ColorStateList(
				CheckedStates,
				new int[]
				{
 					tintColor,
 					tintColor,
 					tintColor,
 					tintColor
				});

			var tintMode = PorterDuff.Mode.SrcIn;

			CompoundButtonCompat.SetButtonTintList(nativeCheckBox, tintList);
			CompoundButtonCompat.SetButtonTintMode(nativeCheckBox, tintMode);
		}
	}
}