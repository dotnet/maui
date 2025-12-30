using System;
using System.Collections.Generic;
using System.Text;
#if ANDROID
using Android.Text;
#endif

namespace Microsoft.Maui
{
	public static class ITextInputExtensions
	{
		public static void UpdateText(this ITextInput textInput, string? text)
		{
			// Even though <null> is technically different to "", it has no
			// functional difference to apps. Thus, hide it.
			var mauiText = textInput.Text ?? string.Empty;
			var platformText = text ?? string.Empty;

			var maxLength = textInput.MaxLength;

			if (maxLength >= 0 && platformText.Length > maxLength)
				platformText = platformText.Substring(0, maxLength);

			if (mauiText != platformText)
				textInput.Text = platformText;
		}

#if __IOS__
		public static bool TextWithinMaxLength(this ITextInput textInput, string? text, Foundation.NSRange range, string replacementString)
		{
			var currLength = text?.Length ?? 0;

			// fix a crash on undo
			if (range.Length + range.Location > currLength)
				return false;

			if (textInput.MaxLength < 0)
				return true;

			var addLength = replacementString?.Length ?? 0;
			var remLength = range.Length;

			var newLength = currLength + addLength - remLength;

			var shouldChange = newLength <= textInput.MaxLength;

			// cut text when user is pasting a text longer that maxlength
			if(!shouldChange && !string.IsNullOrWhiteSpace(replacementString) && replacementString!.Length >= textInput.MaxLength)
				textInput.Text = replacementString!.Substring(0, textInput.MaxLength);

			return shouldChange;
		}
#endif

#if ANDROID
		public static void UpdateText(this ITextInput textInput, TextChangedEventArgs e)
		{
			if (e.Text is Java.Lang.ICharSequence cs)
				textInput.UpdateText(cs.ToString());
			else if (e.Text != null)
				textInput.UpdateText(String.Concat(e.Text));
			else
				textInput.UpdateText((string?)null);
		}
#endif
	}
}
