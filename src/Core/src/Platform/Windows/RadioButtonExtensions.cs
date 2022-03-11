using System;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui
{
	public static class RadioButtonExtensions
	{
		public static void UpdateIsChecked(this RadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.IsChecked = radioButton.IsChecked;
		}

		public static void UpdateTextColor(this Button platformButton, IRadioButton button)
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

		public static void UpdateContent(this RadioButton platformRadioButton, IRadioButton radioButton)
		{
			_ = radioButton.Handler?.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (radioButton.Content is IView view)
				platformRadioButton.Content = view.ToPlatform(radioButton.Handler.MauiContext);
			else
				platformRadioButton.Content = $"{radioButton.Content}";
		}

		public static void UpdateStrokeColor(this RadioButton nativeRadioButton, IRadioButton radioButton)
		{
			if (radioButton.StrokeColor == null)
				return;

			nativeRadioButton.BorderBrush =  radioButton.StrokeColor.ToPlatform();
		}

		public static void UpdateStrokeThickness(this RadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.BorderThickness = radioButton.StrokeThickness <= 0 ? WinUIHelpers.CreateThickness(3) : WinUIHelpers.CreateThickness(radioButton.StrokeThickness);
		}

		public static void UpdateCornerRadius(this RadioButton nativeRadioButton, IRadioButton radioButton)
		{
			if (nativeRadioButton is MauiRadioButton mauiRadioButton)
				mauiRadioButton.BorderRadius = radioButton.CornerRadius;
		}
	}
}