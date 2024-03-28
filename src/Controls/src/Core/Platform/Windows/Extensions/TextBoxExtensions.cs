using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class TextBoxExtensions
	{
		public static void UpdateText(this TextBox platformControl, InputView inputView)
		{
			var hasFocus = platformControl.FocusState != UI.Xaml.FocusState.Unfocused;
			var passwordBox = platformControl as MauiPasswordTextBox;
			var isPassword = passwordBox?.IsPassword ?? false;
			var textTransform = inputView?.TextTransform ?? TextTransform.None;

			// Setting the text causes the cursor to be reset to position zero.
			// So, let's retain the current cursor position and calculate a new cursor
			// position if the text was modified by a Converter.
			var oldText = platformControl.Text ?? string.Empty;
			var newText = TextTransformUtilites.GetTransformedText(
				inputView?.Text,
				isPassword ? TextTransform.None : textTransform
				);

			// Re-calculate the cursor offset position if the text was modified by a Converter.
			// but if the text is being set by code, let's just move the cursor to the end.
			var cursorOffset = newText.Length - oldText.Length;
			int cursorPosition = hasFocus ? platformControl.GetCursorPosition(cursorOffset) : newText.Length;

			if (oldText != newText)
			{
				if (passwordBox is not null)
					MauiPasswordTextBox.Password = newText;
				else
					platformControl.Text = newText;

				platformControl.Select(cursorPosition, 0);
			}
		}
	}

}
