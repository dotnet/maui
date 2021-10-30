using System;
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
			var uiFont = fontManager.GetFont(textStyle.Font);
			textView.Font = uiFont;
		}

		public static void UpdateIsReadOnly(this UITextView textView, IEditor editor)
		{
			textView.UserInteractionEnabled = !editor.IsReadOnly;
		}

		//public static void UpdateCursorPosition(this UITextView textView, IEditor editor)
		//{
		//	var cursorPosition = editor.CursorPosition;
		//	textView

		//}

		[PortHandler]
		public static void UpdateCursorPosition(this UITextView textField, IEditor entry)
			=> UpdateCursorSelection(textField, entry);

		[PortHandler]
		public static void UpdateSelectionLength(this UITextView textField, IEditor entry)
			=> UpdateCursorSelection(textField, entry);

		static void UpdateCursorSelection(this UITextView textField, IEditor entry)
		{
			if (textField == null)
				return;

			if (!entry.IsReadOnly)
			{
				textField.BecomeFirstResponder();
				UITextPosition start = GetSelectionStart(textField, entry, out int startOffset);
				UITextPosition end = GetSelectionEnd(textField, entry, start, startOffset);

				textField.SelectedTextRange = textField.GetTextRange(start, end);
			}
		}

		static UITextPosition GetSelectionStart(UITextView textField, IEditor entry, out int startOffset)
		{
			int cursorPosition = entry.CursorPosition;

			UITextPosition start = textField.GetPosition(textField.BeginningOfDocument, cursorPosition) ?? textField.EndOfDocument;
			startOffset = Math.Max(0, (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, start));

			if (startOffset != cursorPosition)
				entry.CursorPosition = startOffset;

			return start;
		}

		static UITextPosition GetSelectionEnd(UITextView textField, IEditor entry, UITextPosition start, int startOffset)
		{
			int selectionLength = entry.SelectionLength;
			int textFieldLength = textField.Text == null ? 0 : textField.Text.Length;
			// Get the desired range in respect to the actual length of the text we are working with
			UITextPosition end = textField.GetPosition(start, Math.Min(textFieldLength - entry.CursorPosition, selectionLength)) ?? start;
			int endOffset = Math.Max(startOffset, (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, end));

			int newSelectionLength = Math.Max(0, endOffset - startOffset);
			if (newSelectionLength != selectionLength)
				entry.SelectionLength = newSelectionLength;

			return end;
		}
	}
}