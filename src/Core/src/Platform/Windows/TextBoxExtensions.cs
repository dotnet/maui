namespace Microsoft.Maui
{
	public static class TextBoxExtensions
	{
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

		public static void UpdatePlaceholder(this MauiTextBox textBox, IEntry entry)
		{
			textBox.PlaceholderText = entry.Placeholder ?? string.Empty;
		}

		public static void UpdateIsReadOnly(this MauiTextBox textBox, IEntry entry)
		{
			textBox.IsReadOnly = entry.IsReadOnly;
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
