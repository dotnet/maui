using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;
using Windows.Foundation;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() != null)
			{
				handler.PlatformView.Style =
					UI.Xaml.Application.Current.Resources["RadioButtonControlStyle"] as UI.Xaml.Style;
			}
			else
			{
				handler.PlatformView.ClearValue(FrameworkElement.StyleProperty);
			}

			RadioButtonHandler.MapContent(handler, radioButton);

			if ((radioButton.TextTransform is TextTransform.Lowercase or TextTransform.Uppercase) && !string.IsNullOrEmpty(radioButton.Content?.ToString()))
			{
				var transformedText = TextTransformUtilites.GetTransformedText(radioButton.Content.ToString(), radioButton.TextTransform);
				if (handler.PlatformView is Microsoft.UI.Xaml.Controls.RadioButton platformRadioButton)
				{
					platformRadioButton.Content = transformedText;
				}
			}
		}
	}
}
