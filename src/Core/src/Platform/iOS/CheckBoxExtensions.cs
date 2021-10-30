using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this MauiCheckBox nativeCheckBox, ICheckBox check)
		{
			nativeCheckBox.IsChecked = check.IsChecked;
		}

		public static void UpdateForeground(this MauiCheckBox nativeCheckBox, ICheckBox check)
		{
			// For the moment, we're only supporting solid color Paint for the iOS Checkbox
			if (check.Foreground is SolidPaint solid)
			{
				nativeCheckBox.CheckBoxTintColor = solid.Color;
			}
		}
	}
}