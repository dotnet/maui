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
			// Setting the text causes the cursor to be reset to position zero.
			// So, let's retain the current cursor position and calculate a new cursor
			// position if the text was modified by a Converter.
			var oldText = platformControl.Text ?? string.Empty;
			var newText = inputView?.Text ?? string.Empty;

			// Calculate the cursor offset position if the text was modified by a Converter.
			var cursorOffset = newText.Length - oldText.Length;
			int cursorPosition = platformControl.GetCursorPosition(cursorOffset);

			var textTransform = inputView?.TextTransform ?? TextTransform.None;

			if (platformControl is MauiPasswordTextBox passwordBox)
				passwordBox.Password = TextTransformUtilites.GetTransformedText(newText, passwordBox.IsPassword ? TextTransform.None : textTransform);
			else
				platformControl.Text = TextTransformUtilites.GetTransformedText(newText, textTransform);

			platformControl.Select(cursorPosition, 0);
		}
	}

}
