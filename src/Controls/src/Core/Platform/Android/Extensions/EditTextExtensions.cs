using Android.Text;
using Android.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Android;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls.Platform
{
	public static class EditTextExtensions
	{
		public static void UpdateImeOptions(this EditText editText, Entry entry)
		{
			var imeOptions = entry.OnThisPlatform().ImeOptions().ToPlatform();

			editText.ImeOptions = imeOptions;
		}

		public static void UpdateText(this EditText editText, InputView inputView)
		{
			bool isPasswordEnabled =
				(editText.InputType & InputTypes.TextVariationPassword) == InputTypes.TextVariationPassword ||
				(editText.InputType & InputTypes.NumberVariationPassword) == InputTypes.NumberVariationPassword;

			// Setting the text causes the cursor to be reset to position zero.
			// So, let's retain the current cursor position and calculate a new cursor
			// position if the text was modified by a Converter.
			var oldText = editText.Text ?? string.Empty;
			var newText = TextTransformUtilites.GetTransformedText(
				inputView?.Text,
				isPasswordEnabled ? TextTransform.None : inputView.TextTransform
				);

			// Re-calculate the cursor offset position if the text was modified by a Converter.
			// but if the text is being set by code, let's just move the cursor to the end.
			var cursorOffset = newText.Length - oldText.Length;
			int cursorPosition = editText.IsFocused ? editText.GetCursorPosition(cursorOffset) : newText.Length;

			if (oldText != newText)
				editText.Text = newText;

			editText.SetSelection(cursorPosition, cursorPosition);
		}
	}
}