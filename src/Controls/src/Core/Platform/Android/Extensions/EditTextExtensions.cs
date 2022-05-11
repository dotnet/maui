using Android.Text;
using Android.Widget;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Android;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

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
			var oldText = editText?.Text ?? string.Empty;
			var newText = inputView?.Text ?? string.Empty;

			// Setting the text causes the cursor to be reset to position zero.
			// So, let's retain the current cursor position and calculate a new cursor
			// position if the text was modified by a Converter.
			int currentCursorPosition = editText.SelectionStart;

			// Calculate the cursor offset position if the text was modified by a Converter.
			var cursorOffset = newText.Length - oldText.Length;

			bool isPasswordEnabled =
				(editText.InputType & InputTypes.TextVariationPassword) == InputTypes.TextVariationPassword ||
				(editText.InputType & InputTypes.NumberVariationPassword) == InputTypes.NumberVariationPassword;

			editText.Text = TextTransformUtilites.GetTransformedText(newText, isPasswordEnabled ? TextTransform.None : inputView.TextTransform);

			editText.SetSelection(currentCursorPosition + cursorOffset);
		}
	}
}