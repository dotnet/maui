using UIKit;

namespace Microsoft.Maui
{
	public static class EditorExtensions
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
			var textAttr = textView.AttributedText.AddCharacterSpacing(editor.Text, editor.CharacterSpacing);

			if (textAttr != null)
				textView.AttributedText = textAttr;

			// TODO: Include AttributedText to Label Placeholder
		}
		
		public static void UpdatePredictiveText(this UITextView textView, IEditor editor)
		{
			textView.AutocorrectionType = editor.IsTextPredictionEnabled 
				? UITextAutocorrectionType.Yes : UITextAutocorrectionType.No;
		}
	}
}