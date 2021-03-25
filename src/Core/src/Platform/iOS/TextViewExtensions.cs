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

		public static void UpdateCharacterSpacing(this UITextView textView, IEditor editor)
		{
			var textAttr = textView.AttributedText?.WithCharacterSpacing(editor.CharacterSpacing);

			if (textAttr != null)
				textView.AttributedText = textAttr;

			// TODO: Include AttributedText to Label Placeholder
		}

		public static void UpdateMaxLength(this UITextView textView, IEditor editor)
		{
			var currentControlText = textView.Text;

			if (currentControlText?.Length > editor.MaxLength)
				textView.Text = currentControlText.Substring(0, editor.MaxLength);
		}

		public static void UpdatePredictiveText(this UITextView textView, IEditor editor)
		{
			textView.AutocorrectionType = editor.IsTextPredictionEnabled
				? UITextAutocorrectionType.Yes : UITextAutocorrectionType.No;
		}

		public static void UpdateFont(this UITextView textView, IEditor editor, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(editor.Font);
			textView.Font = uiFont;
		}

		public static void UpdateIsReadOnly(this UITextView textView, IEditor editor)
		{
			textView.UserInteractionEnabled = !editor.IsReadOnly;
		}
	}
}