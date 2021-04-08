using UIKit;

namespace Microsoft.Maui
{
	public static class TextViewExtensions
	{
		public static void UpdateText(this UITextView textView, IEditor editor)
		{
			string text = editor.Text;

			if (textView.Text != text)
			{
				textView.Text = text;
			}
		}

		public static void UpdateCharacterSpacing(this UITextView textView, ITextStyle textStyle)
		{
			var textAttr = textView.AttributedText?.WithCharacterSpacing(textStyle.CharacterSpacing);
			if (textAttr != null)
				textView.AttributedText = textAttr;

			// TODO: Include AttributedText to Label Placeholder
		}

		public static void UpdateMaxLength(this UITextView textView, IEditor editor)
		{
			var newText = textView.AttributedText.TrimToMaxLength(editor.MaxLength);
			if (newText != null && textView.AttributedText != newText)
				textView.AttributedText = newText;
		}

		public static void UpdatePredictiveText(this UITextView textView, IEditor editor)
		{
			textView.AutocorrectionType = editor.IsTextPredictionEnabled
				? UITextAutocorrectionType.Yes : UITextAutocorrectionType.No;
		}

		public static void UpdateFont(this UITextView textView, ITextStyle textStyle, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(textStyle.Font);
			textView.Font = uiFont;
		}

		public static void UpdateIsReadOnly(this UITextView textView, IEditor editor)
		{
			textView.UserInteractionEnabled = !editor.IsReadOnly;
		}

		public static void UpdateKeyboard(this UITextView textView, IEditor editor)
		{
			var keyboard = editor.Keyboard;

			textView.ApplyKeyboard(keyboard);

			if (keyboard is not CustomKeyboard)
			{
				// TODO: IsSpellCheckEnabled handling must be here.

				if (!editor.IsTextPredictionEnabled)
				{
					textView.AutocorrectionType = UITextAutocorrectionType.No;
				}
			}

			textView.ReloadInputViews();
		}
	}
}