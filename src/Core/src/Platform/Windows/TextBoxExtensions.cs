namespace Microsoft.Maui
{
	public static class TextBoxExtensions
	{
		public static void UpdateText(this MauiTextBox nativeControl, IEditor editor)
		{
			string newText = editor.Text;

			if (nativeControl.Text == newText)
				return;

			nativeControl.Text = newText;

			if (!string.IsNullOrEmpty(nativeControl.Text))
				nativeControl.SelectionStart = nativeControl.Text.Length;
		}

		public static void UpdateText(this MauiTextBox textBox, IEntry entry)
		{
			textBox.Text = entry.Text;
		}

		public static void UpdateTextColor(this MauiTextBox textBox, ITextStyle textStyle)
		{
			if (textStyle.TextColor == null)
				return;

			var brush = textStyle.TextColor.ToNative();
			textBox.Foreground = brush;
			textBox.ForegroundFocusBrush = brush;
		}

		public static void UpdateReturnType(this MauiTextBox textBox, IEntry entry)
		{
			textBox.InputScope = entry.ReturnType.ToNative();
    }

    public static void UpdatePlaceholder(this MauiTextBox textBox, IEditor editor)
		{
			textBox.PlaceholderText = editor.Placeholder ?? string.Empty;
		}

		public static void UpdatePlaceholder(this MauiTextBox textBox, IEntry entry)
		{
			textBox.PlaceholderText = entry.Placeholder ?? string.Empty;
		}

		public static void UpdateFont(this MauiTextBox nativeControl, IText text, IFontManager fontManager) =>		
			nativeControl.UpdateFont(text.Font, fontManager);

		public static void UpdateFont(this MauiTextBox nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.ToFontStyle();
			nativeControl.FontWeight = font.ToFontWeight();
		}

		public static void UpdateIsReadOnly(this MauiTextBox textBox, IEditor editor)
		{
			textBox.IsReadOnly = editor.IsReadOnly;
		}

		public static void UpdateIsReadOnly(this MauiTextBox textBox, IEntry entry)
		{
			textBox.IsReadOnly = entry.IsReadOnly;
		}

		public static void UpdateMaxLength(this MauiTextBox textBox, IEditor editor)
		{
			textBox.MaxLength = editor.MaxLength;

			var currentControlText = textBox.Text;

			if (currentControlText.Length > editor.MaxLength)
				textBox.Text = currentControlText.Substring(0, editor.MaxLength);
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
	}
}
