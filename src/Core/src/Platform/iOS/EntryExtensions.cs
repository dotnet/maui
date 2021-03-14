using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui
{
	public static class EntryExtensions
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

		public static void UpdateIsTextPredictionEnabled(this UITextField textField, IEntry entry)
		{
			if (entry.IsTextPredictionEnabled)
				textField.AutocorrectionType = UITextAutocorrectionType.Yes;
			else
				textField.AutocorrectionType = UITextAutocorrectionType.No;
		}

		public static void UpdatePlaceholder(this UITextField textField, IEntry entry)
		{
			textField.Placeholder = entry.Placeholder;
		}

		public static void UpdateIsReadOnly(this UITextField textField, IEntry entry)
		{
			textField.UserInteractionEnabled = !entry.IsReadOnly;
		}

		public static void UpdateKeyboard(this UITextField textField, IEntry entry)
		{
			var keyboard = entry.Keyboard;

			textField.ApplyKeyboard(keyboard);
			textField.ReloadInputViews();
		}
			
		public static void UpdateFont(this UITextField textField, IEntry entry, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(entry.Font);
			textField.Font = uiFont;
		}
	}
}