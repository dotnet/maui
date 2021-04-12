namespace Microsoft.Maui
{
	public static class TextBoxExtensions
	{
		public static void UpdateText(this MauiTextBox textBox, IEntry entry)
		{
			textBox.Text = entry.Text;
		}

		public static void UpdateIsPassword(this MauiTextBox textBox, IEntry entry)
		{
			textBox.IsPassword = entry.IsPassword;
		}
	}
}