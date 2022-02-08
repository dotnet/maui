using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Media;

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
			var tintBrush = check.Foreground?.ToPlatform();

			if (tintBrush == null)
			{
				platformCheckBox.TintBrush = new SolidColorBrush(UI.Colors.Black);
				return;
			}

			platformCheckBox.TintBrush = tintBrush;
		}
	}
}