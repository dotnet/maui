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

		public static void UpdateText(this EditText editText, InputView inputView)
		{
			bool isPasswordEnabled =
				(editText.InputType & InputTypes.TextVariationPassword) == InputTypes.TextVariationPassword ||
				(editText.InputType & InputTypes.NumberVariationPassword) == InputTypes.NumberVariationPassword;

			var oldText = editText.Text ?? string.Empty;
			var newText = TextTransformUtilites.GetTransformedText(inputView?.Text,
					isPasswordEnabled ? TextTransform.None : inputView.TextTransform);

			if (oldText != newText)
			{
				// Update the text and keep the cursor position 
				editText.SetTextKeepState(newText);
			}
		}
	}
}