namespace Microsoft.Maui
{
	public static class TextBoxExtensions
	{
		public static void UpdateText(this MauiTextBox textBox, IEntry entry)
		{
			textBox.Text = entry.Text;
		}

		public static void UpdateForeground(this MauiTextBox textView, ITextStyle textStyle)
		{
			if (textStyle.Foreground == null)
				return;

			var brush = textStyle.Foreground.ToNative();
			textView.Foreground = brush;
			textView.ForegroundFocusBrush = brush;
		}
	}
}