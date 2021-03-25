using UIKit;

namespace Microsoft.Maui
{
	public static class TextFieldExtensions
	{
		public static void UpdateText(this UITextField textField, IEntry entry)
		{
			textField.Text = entry.Text;
		}

		public static void UpdateTextColor(this UITextField textField, IEntry entry)
		{
			textField.UpdateTextColor(entry, null);
		}

		public static void UpdateTextColor(this UITextField textField, IEntry entry, UIColor? defaultTextColor)
		{
			if (entry.TextColor == Color.Default)
			{
				if (defaultTextColor != null)
					textField.TextColor = defaultTextColor;
			}
			else
				textField.TextColor = entry.TextColor.ToNative();
		}

		public static void UpdateIsPassword(this UITextField textField, IEntry entry)
		{
			if (entry.IsPassword && textField.IsFirstResponder)
			{
				textField.Enabled = false;
				textField.SecureTextEntry = true;
				textField.Enabled = entry.IsEnabled;
				textField.BecomeFirstResponder();
			}
			else
				textField.SecureTextEntry = entry.IsPassword;
		}

		public static void UpdateHorizontalTextAlignment(this UITextField textField, ITextAlignment textAlignment)
		{
			// We don't have a FlowDirection yet, so there's nothing to pass in here. 
			// TODO: Update this when FlowDirection is available 
			// (or update the extension to take an ILabel instead of an alignment and work it out from there) 
			textField.TextAlignment = textAlignment.HorizontalTextAlignment.ToNative(true);
		}

		public static void UpdateIsTextPredictionEnabled(this UITextField textField, IEntry entry)
		{
			if (entry.IsTextPredictionEnabled)
				textField.AutocorrectionType = UITextAutocorrectionType.Yes;
			else
				textField.AutocorrectionType = UITextAutocorrectionType.No;
		}

		public static void UpdateMaxLength(this UITextField textField, IEntry entry)
		{
			var currentControlText = textField.Text;

			if (currentControlText?.Length > entry.MaxLength)
				textField.Text = currentControlText.Substring(0, entry.MaxLength);
		}

		public static void UpdatePlaceholder(this UITextField textField, IEntry entry)
		{
			textField.Placeholder = entry.Placeholder;
		}

		public static void UpdateIsReadOnly(this UITextField textField, IEntry entry)
		{
			textField.UserInteractionEnabled = !entry.IsReadOnly;
		}

		public static void UpdateFont(this UITextField textField, IEntry entry, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(entry.Font);
			textField.Font = uiFont;
		}

		public static void UpdateReturnType(this UITextField textField, IEntry entry)
		{
			textField.ReturnKeyType = entry.ReturnType.ToNative();
		}

		public static void UpdateCharacterSpacing(this UITextField textField, IText textView)
		{
			var textAttr = textField.AttributedText?.WithCharacterSpacing(textView.CharacterSpacing);

			if (textAttr != null)
				textField.AttributedText = textAttr;
		}

		public static void UpdateCharacterSpacing(this UITextField textField, IEntry textView)
		{
			var textAttr = textField.AttributedText?.WithCharacterSpacing(textView.CharacterSpacing);

			if (textAttr != null)
				textField.AttributedText = textAttr;
		}

		public static void UpdateFont(this UITextField textField, IText textView, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(textView.Font);
			textField.Font = uiFont;
		}

		public static void UpdateClearButtonVisibility(this UITextField textField, IEntry entry)
		{
			textField.ClearButtonMode = entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing ? UITextFieldViewMode.WhileEditing : UITextFieldViewMode.Never;
		}
	}
}