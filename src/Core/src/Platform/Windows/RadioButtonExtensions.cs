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

		public static void UpdateTextColor(this Button platformButton, ITextStyle button)
		{
			var brush = button.TextColor?.ToPlatform();

			if (brush is null)
			{
				// Windows.Foundation.UniversalApiContract < 5
				platformButton.Resources.Remove("RadioButtonForeground");
				platformButton.Resources.Remove("RadioButtonForegroundPointerOver");
				platformButton.Resources.Remove("RadioButtonForegroundPressed");
				platformButton.Resources.Remove("RadioButtonForegroundDisabled");

				// Windows.Foundation.UniversalApiContract >= 5
				platformButton.ClearValue(RadioButton.ForegroundProperty);
			}
			else
			{
				// Windows.Foundation.UniversalApiContract < 5
				platformButton.Resources["RadioButtonForeground"] = brush;
				platformButton.Resources["RadioButtonForegroundPointerOver"] = brush;
				platformButton.Resources["RadioButtonForegroundPressed"] = brush;
				platformButton.Resources["RadioButtonForegroundDisabled"] = brush;

				// Windows.Foundation.UniversalApiContract >= 5
				platformButton.Foreground = brush;
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