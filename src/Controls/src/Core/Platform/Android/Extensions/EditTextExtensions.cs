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
			// Therefore if:
			// User Types => VirtualView Updated => Triggers Native Update
			// Then it will cause the cursor to reset to position zero as the user typed
			if (value != editText.Text)
				editText.Text = value;
		}
	}
}
