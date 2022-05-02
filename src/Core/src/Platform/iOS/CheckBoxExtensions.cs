using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this MauiCheckBox platformCheckBox, ICheckBox check)
		{
			platformCheckBox.IsChecked = check.IsChecked;
		}

		public static void UpdateForeground(this MauiCheckBox platformCheckBox, ICheckBox check)
		{
			// For the moment, we're only supporting solid color Paint for the iOS Checkbox
			if (check.Foreground is SolidPaint solid)
			{
				platformCheckBox.CheckBoxTintColor = solid.Color;
			}
		}
	}
}