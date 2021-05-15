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
	}
}