using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class TextViewExtensions
	{
		public static void UpdateText(this UITextView textView, IEditor editor)
		{
			string text = editor.Text;

			if (textView.Text != text)
				textView.Text = text;
		}

		public static void UpdateTextColor(this UITextView textView, IEditor editor)
		{
			var textColor = editor.TextColor;

			if (textColor == null)
				textView.TextColor = ColorExtensions.LabelColor;
			else
				textView.TextColor = textColor.ToPlatform();
		}

		public static void UpdateCharacterSpacing(this UITextView textView, ITextStyle textStyle)
		{
			var textAttr = textView.AttributedText?.WithCharacterSpacing(textStyle.CharacterSpacing);
			if (textAttr != null)
				textView.AttributedText = textAttr;

			if (textView is MauiTextView mauiTextView)
			{
				var phAttr = mauiTextView.AttributedPlaceholderText?.WithCharacterSpacing(textStyle.CharacterSpacing);
				if (phAttr != null)
					mauiTextView.AttributedPlaceholderText = phAttr;
			}
		}

		public static void UpdateMaxLength(this UITextView textView, IEditor editor)
		{
			var newText = textView.AttributedText.TrimToMaxLength(editor.MaxLength);
			if (newText != null && textView.AttributedText != newText)
				textView.AttributedText = newText;
		}

		public static void UpdateIsTextPredictionEnabled(this UITextView textView, IEditor editor)
		{
			if (editor.IsTextPredictionEnabled)
				textView.AutocorrectionType = UITextAutocorrectionType.Yes;
			else
				textView.AutocorrectionType = UITextAutocorrectionType.No;
		}

		public static void UpdateIsSpellCheckEnabled(this UITextView textView, IEditor editor)
		{
			if (editor.IsSpellCheckEnabled)
				textView.SpellCheckingType = UITextSpellCheckingType.Yes;
			else
				textView.SpellCheckingType = UITextSpellCheckingType.No;
		}

		public static void UpdateFont(this UITextView textView, ITextStyle textStyle, IFontManager fontManager)
		{
			var font = textStyle.Font;
			var uiFont = fontManager.GetFont(font, UIFont.LabelFontSize);
			textView.Font = uiFont;
		}

		public static void UpdateIsReadOnly(this UITextView textView, IEditor editor)
		{
			textView.UpdateEditable(editor);
		}

		public static void UpdateIsEnabled(this UITextView textView, IEditor editor)
		{
			textView.UpdateEditable(editor);
		}

		internal static void UpdateEditable(this UITextView textView, IEditor editor)
		{
			var isEditable = editor.IsEnabled && !editor.IsReadOnly;

			textView.Editable = isEditable;

			// If the input accessory view is set, we need to hide it when the editor is read-only
			// otherwise it will appear when the editor recieves focus.
			if (textView.InputAccessoryView is { } view)
				view.Hidden = !isEditable;
		}

		public static void UpdateKeyboard(this UITextView textView, IEditor editor)
		{
			var keyboard = editor.Keyboard;

			textView.ApplyKeyboard(keyboard);

			if (keyboard is not CustomKeyboard)
			{
				textView.UpdateIsTextPredictionEnabled(editor);
				textView.UpdateIsSpellCheckEnabled(editor);
			}

			textView.ReloadInputViews();
		}

		public static void UpdateCursorPosition(this UITextView textView, IEditor editor)
		{
			var selectedTextRange = textView.SelectedTextRange;
			if (selectedTextRange == null)
				return;
			if (textView.GetOffsetFromPosition(textView.BeginningOfDocument, selectedTextRange.Start) != editor.CursorPosition)
				UpdateCursorSelection(textView, editor);
		}

		public static void UpdateSelectionLength(this UITextView textView, IEditor editor)
		{
			var selectedTextRange = textView.SelectedTextRange;
			if (selectedTextRange == null)
				return;
			if (textView.GetOffsetFromPosition(selectedTextRange.Start, selectedTextRange.End) != editor.SelectionLength)
				UpdateCursorSelection(textView, editor);
		}

		public static void UpdateHorizontalTextAlignment(this UITextView textView, IEditor editor)
		{
			textView.TextAlignment = editor.HorizontalTextAlignment.ToPlatformHorizontal(textView.EffectiveUserInterfaceLayoutDirection);
		}

		public static void UpdateVerticalTextAlignment(this MauiTextView textView, IEditor editor)
		{
			textView.VerticalTextAlignment = editor.VerticalTextAlignment;
		}

		public static void UpdatePlaceholder(this MauiTextView textView, IEditor editor) =>
			textView.PlaceholderText = editor.Placeholder;

		public static void UpdatePlaceholderColor(this MauiTextView textView, IEditor editor) =>
			textView.PlaceholderTextColor = editor.PlaceholderColor?.ToPlatform() ?? ColorExtensions.PlaceholderColor;

		static void UpdateCursorSelection(this UITextView textView, IEditor editor)
		{
			if (!editor.IsReadOnly)
			{
				UITextPosition start = GetSelectionStart(textView, editor, out int startOffset);
				UITextPosition end = GetSelectionEnd(textView, editor, start, startOffset);

				textView.SelectedTextRange = textView.GetTextRange(start, end);
			}
		}

		static UITextPosition GetSelectionStart(UITextView textView, IEditor editor, out int startOffset)
		{
			int cursorPosition = editor.CursorPosition;

			UITextPosition start = textView.GetPosition(textView.BeginningOfDocument, cursorPosition) ?? textView.EndOfDocument;
			startOffset = Math.Max(0, (int)textView.GetOffsetFromPosition(textView.BeginningOfDocument, start));

			if (startOffset != cursorPosition)
				editor.CursorPosition = startOffset;

			return start;
		}

		static UITextPosition GetSelectionEnd(UITextView textView, IEditor editor, UITextPosition start, int startOffset)
		{
			int selectionLength = editor.SelectionLength;
			int textFieldLength = textView.Text == null ? 0 : textView.Text.Length;
			// Get the desired range in respect to the actual length of the text we are working with
			UITextPosition end = textView.GetPosition(start, Math.Min(textFieldLength - editor.CursorPosition, selectionLength)) ?? start;
			int endOffset = Math.Max(startOffset, (int)textView.GetOffsetFromPosition(textView.BeginningOfDocument, end));

			int newSelectionLength = Math.Max(0, endOffset - startOffset);
			if (newSelectionLength != selectionLength)
				editor.SelectionLength = newSelectionLength;

			return end;
		}
	}
}
