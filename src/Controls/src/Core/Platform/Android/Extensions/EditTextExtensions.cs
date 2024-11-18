#nullable disable
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

		static (string oldText, string newText) GetTexts(EditText editText, InputView inputView)
		{
			var oldText = editText.Text ?? string.Empty;

			var inputType = editText.InputType;

			bool isPasswordEnabled =
				(inputType & InputTypes.TextVariationPassword) == InputTypes.TextVariationPassword ||
				(inputType & InputTypes.NumberVariationPassword) == InputTypes.NumberVariationPassword;

			var newText = TextTransformUtilites.GetTransformedText(inputView?.Text,
					isPasswordEnabled ? TextTransform.None : inputView.TextTransform);

			return (oldText, newText);
		}

		public static void UpdateText(this EditText editText, InputView inputView)
		{
			(var oldText, var newText) = GetTexts(editText, inputView);

			if (oldText != newText)
			{
				editText.Text = newText;

				// When updating from xplat->plat, we set the selection (cursor) to the end of the text
				if (newText.Length <= editText.Text.Length)
					editText.SetSelection(newText.Length);
				else
					editText.SetSelection(editText.Text.Length);
			}
		}

		internal static void UpdateTextFromPlatform(this EditText editText, InputView inputView)
		{
			(var oldText, var newText) = GetTexts(editText, inputView);

			if (oldText != newText)
			{
				// This update is happening while inputting text into the EditText, so we want to avoid 
				// resettting the cursor position and selection
				editText.SetTextKeepState(newText);
			}
		}
	}
}