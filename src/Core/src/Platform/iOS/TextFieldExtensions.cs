using UIKit;

namespace Microsoft.Maui
{
	public static class TextFieldExtensions
	{
		public static void UpdateText(this UITextField textField, IEntry entry)
		{
			textField.Text = entry.Text;
		}

		public static void UpdateTextColor(this UITextField textField, ITextStyle textStyle, UIColor? defaultTextColor = null)
		{
			// Default value of color documented to be black in iOS docs

			var textColor = textStyle.TextColor;
			textField.TextColor = textColor.ToNative(defaultTextColor ?? ColorExtensions.LabelColor);
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
			var newText = textField.AttributedText.TrimToMaxLength(entry.MaxLength);
			if (newText != null && textField.AttributedText != newText)
				textField.AttributedText = newText;
		}

		public static void UpdatePlaceholder(this UITextField textField, IEntry entry)
		{
			textField.Placeholder = entry.Placeholder;
		}

		public static void UpdateIsReadOnly(this UITextField textField, IEntry entry)
		{
			textField.UserInteractionEnabled = !entry.IsReadOnly;
		}

		public static void UpdateFont(this UITextField textField, ITextStyle textStyle, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(textStyle.Font, UIFont.LabelFontSize);
			textField.Font = uiFont;
		}

		public static void UpdateReturnType(this UITextField textField, IEntry entry)
		{
			textField.ReturnKeyType = entry.ReturnType.ToNative();
		}

		public static void UpdateCharacterSpacing(this UITextField textField, ITextStyle textStyle)
		{
			var textAttr = textField.AttributedText?.WithCharacterSpacing(textStyle.CharacterSpacing);
			if (textAttr != null)
				textField.AttributedText = textAttr;
		}

		public static void UpdateKeyboard(this UITextField textField, IEntry entry)
		{
			var keyboard = entry.Keyboard;

			textField.ApplyKeyboard(keyboard);

			if (keyboard is not CustomKeyboard)
				textField.UpdateIsTextPredictionEnabled(entry);

			textField.ReloadInputViews();
		}

		public static void UpdateClearButtonVisibility(this UITextField textField, IEntry entry)
		{
			textField.ClearButtonMode = entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing ? UITextFieldViewMode.WhileEditing : UITextFieldViewMode.Never;
		}
	}
}