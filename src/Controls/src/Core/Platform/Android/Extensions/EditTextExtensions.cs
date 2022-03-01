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
			if (value == editText.Text)
				return;

			int selectionStart = editText.SelectionStart;
			editText.Text = value;
			editText.SetSelection(selectionStart);
		}
	}
}
