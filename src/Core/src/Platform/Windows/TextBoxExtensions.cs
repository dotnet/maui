namespace Microsoft.Maui
{
	public static class TextBoxExtensions
	{
		public static void UpdateTextColor(this MauiTextBox textView, ITextStyle textStyle)
		{
			if (textStyle.TextColor == null)
				return;

			var brush = textStyle.TextColor.ToNative();
			textView.Foreground = brush;
			textView.ForegroundFocusBrush = brush;
		}
		
		public static void UpdateText(this MauiTextBox nativeControl, IEditor editor)
		{
			string newText = editor.Text;

			if (nativeControl.Text == newText)
				return;
			
			nativeControl.Text = newText;
			nativeControl.SelectionStart = nativeControl.Text.Length;
		}

		public static void UpdateText(this MauiTextBox nativeControl, IEntry entry)
		{
			nativeControl.Text = entry.Text;
		}

		public static void UpdatePlaceholder(this MauiTextBox nativeControl, IEditor editor)
		{
			nativeControl.PlaceholderText = editor.Placeholder ?? string.Empty;
		}

		public static void UpdateFont(this MauiTextBox nativeControl, IText text, IFontManager fontManager) =>
			nativeControl.UpdateFont(text.Font, fontManager);

		public static void UpdateFont(this MauiTextBox nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.FontAttributes.ToFontStyle();
			nativeControl.FontWeight = font.FontAttributes.ToFontWeight();
		}

		public static void UpdateMaxLength(this MauiTextBox nativeControl, IEditor editor)
		{
			nativeControl.MaxLength = editor.MaxLength;

			var currentControlText = nativeControl.Text;

			if (currentControlText.Length > editor.MaxLength)
				nativeControl.Text = currentControlText.Substring(0, editor.MaxLength);
		}
    
		public static void UpdateMaxLength(this MauiTextBox textBox, IEntry entry)
		{
			var maxLength = entry.MaxLength;

			if (maxLength == -1)
				maxLength = int.MaxValue;

			textBox.MaxLength = maxLength;

			var currentControlText = textBox.Text;

			if (currentControlText.Length > maxLength)
				textBox.Text = currentControlText.Substring(0, maxLength);
		}
    
		public static void UpdateIsReadOnly(this MauiTextBox nativeControl, IEditor editor)
		{
			nativeControl.IsReadOnly = editor.IsReadOnly;
    }
	}
}
