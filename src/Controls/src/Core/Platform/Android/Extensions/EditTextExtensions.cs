#nullable disable
using System;
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

			var newText = TextTransformUtilities.GetTransformedText(inputView?.Text,
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
				// Use Editable.Replace() instead of SetTextKeepState() to avoid re-entering setText()
				// inside afterTextChanged.
				// SetTextKeepState() causes EmojiTextWatcher to crash with stale span positions
				// when a StringFormat binding updates the text programmatically.
				// https://github.com/dotnet/maui/issues/25728
				var editable = editText.EditableText;

				if (editable is not null)
				{
					// Preserve cursor position across the replace (matches SetTextKeepState behavior)
					int selStart = editText.SelectionStart;
					int selEnd = editText.SelectionEnd;

					editable.Replace(0, editable.Length(), newText);

					// Clamp selection to new text length before restoring
					int len = editText.Text?.Length ?? 0;
					selStart = Math.Min(selStart, len);
					selEnd = Math.Min(selEnd, len);

					if (selStart >= 0 && selEnd >= selStart)
					{
						editText.SetSelection(selStart, selEnd);
					}
				}
				else
				{
					editText.SetTextKeepState(newText);
				}
			}
		}
	}
}