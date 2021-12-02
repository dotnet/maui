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
				textView.TextColor = textColor.ToNative();
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

		public static void UpdateFont(this UITextView textView, ITextStyle textStyle, IFontManager fontManager)
		{
			var font = textStyle.Font;
			var uiFont = fontManager.GetFont(font, UIFont.LabelFontSize);
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
				textView.UpdateIsTextPredictionEnabled(editor);

			textView.ReloadInputViews();
		}

		public static void UpdateHorizontalTextAlignment(this UITextView textView, ITextAlignment textAlignment)
		{
			// We don't have a FlowDirection yet, so there's nothing to pass in here. 
			// TODO ezhart Update this when FlowDirection is available 
			// (or update the extension to take an IEditor instead of an alignment and work it out from there) 
			textView.TextAlignment = textAlignment.HorizontalTextAlignment.ToNative(true);
		}

		public static void UpdatePlaceholder(this MauiTextView textView, IEditor editor) =>
			textView.PlaceholderText = editor.Placeholder;

		public static void UpdatePlaceholderColor(this MauiTextView textView, IEditor editor) =>
			textView.PlaceholderTextColor = editor.PlaceholderColor?.ToNative() ?? ColorExtensions.PlaceholderColor;
	}
}