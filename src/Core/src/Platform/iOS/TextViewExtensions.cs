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

		public static void UpdateTextColor(this UITextView textView, IEditor editor)
		{
			var textColor = editor.TextColor;

			if (textColor == null)
				textView.TextColor = ColorExtensions.LabelColor;
			else
				textView.TextColor = textColor.ToNative();
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
			var uiFont = fontManager.GetFont(textStyle.Font, UIFont.LabelFontSize);
			textView.Font = uiFont;
		}

		public static void UpdateIsReadOnly(this UITextView textView, IEditor editor)
		{
			textView.UserInteractionEnabled = !editor.IsReadOnly;
		}
	}
}