using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Media;

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
			var tintBrush = check.Foreground?.ToNative();

			if (tintBrush == null)
			{
				nativeCheckBox.TintBrush = new SolidColorBrush(UI.Colors.Black);
				return;
			}

			nativeCheckBox.TintBrush = tintBrush;
		}
	}
}