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
	}
}
