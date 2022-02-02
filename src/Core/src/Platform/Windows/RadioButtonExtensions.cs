using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class RadioButtonExtensions
	{
		public static void UpdateIsChecked(this RadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.IsChecked = radioButton.IsChecked;
		}

		public static void UpdateTextColor(this Button nativeButton, ITextStyle button)
		{
			var brush = button.TextColor?.ToNative();

			if (brush is null)
			{
				// Windows.Foundation.UniversalApiContract < 5
				nativeButton.Resources.Remove("RadioButtonForeground");
				nativeButton.Resources.Remove("RadioButtonForegroundPointerOver");
				nativeButton.Resources.Remove("RadioButtonForegroundPressed");
				nativeButton.Resources.Remove("RadioButtonForegroundDisabled");

				// Windows.Foundation.UniversalApiContract >= 5
				nativeButton.ClearValue(RadioButton.ForegroundProperty);
			}
			else
			{
				// Windows.Foundation.UniversalApiContract < 5
				nativeButton.Resources["RadioButtonForeground"] = brush;
				nativeButton.Resources["RadioButtonForegroundPointerOver"] = brush;
				nativeButton.Resources["RadioButtonForegroundPressed"] = brush;
				nativeButton.Resources["RadioButtonForegroundDisabled"] = brush;

				// Windows.Foundation.UniversalApiContract >= 5
				nativeButton.Foreground = brush;
			}
		}

		public static void UpdateContent(this RadioButton nativeRadioButton, IRadioButton radioButton)
		{
			_ = radioButton.Handler?.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (radioButton.Content is IView view)
				nativeRadioButton.Content = view.ToPlatform(radioButton.Handler.MauiContext);
			else
				nativeRadioButton.Content = $"{radioButton.Content}";
		}
			
	}
}