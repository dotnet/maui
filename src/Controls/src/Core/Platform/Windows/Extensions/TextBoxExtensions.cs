#nullable enable
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class TextBoxExtensions
	{
		public static void UpdateText(this TextBox platformControl, InputView inputView)
		{
			if (platformControl is MauiPasswordTextBox passwordBox && passwordBox.IsPassword)
				passwordBox.Password = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
			else
				platformControl.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}
	}

}
