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
			// Is UpdateText being called only to transform the text
			// that's already set on the platform element?
			// If so then we want to retain the cursor position
			bool transformingPlatformText =
				(editText.Text == inputView.Text);

			var value = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);

			if (!transformingPlatformText)
			{
				editText.Text = value;
			}
			else
			{
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
}