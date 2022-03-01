using Android.Widget;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class EditTextExtensions
	{
		public static void UpdateText(this EditText editText, InputView inputView)
		{
			var value = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);

			// Setting the text causes the cursor to reset to position zero
			// so if we are transforming the text and then setting it to a 
			// new value then we need to retain the cursor position
			if (value == inputView.Text)
				return;

			int selectionStart = 0;

			// This means the user is typing at the end of the string
			if (selectionStart == value.Length)
				selectionStart = value.Length;
			// Users is typing at the start/middle of the string
			else
				selectionStart = editText.SelectionStart;

			editText.Text = value;
			editText.SetSelection(selectionStart);
		}
	}
}
