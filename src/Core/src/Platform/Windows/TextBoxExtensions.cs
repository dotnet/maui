using Microsoft.UI.Xaml.Controls;

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
		
		public static void UpdateText(this TextBox nativeControl, IEditor editor)
		{
			string newText = editor.Text;

			if (nativeControl.Text == newText)
				return;
			
			nativeControl.Text = newText;
			nativeControl.SelectionStart = nativeControl.Text.Length;
		}

		public static void UpdatePlaceholder(this TextBox nativeControl, IEditor editor)
		{
			nativeControl.PlaceholderText = editor.Placeholder ?? string.Empty;
		}
		public static void UpdateFont(this TextBox nativeControl, IText text, IFontManager fontManager) =>
			nativeControl.UpdateFont(text.Font, fontManager);

		public static void UpdateFont(this TextBox nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.FontAttributes.ToFontStyle();
			nativeControl.FontWeight = font.FontAttributes.ToFontWeight();
		}

		public static void UpdateMaxLength(this TextBox nativeControl, IEditor editor)
		{
			nativeControl.MaxLength = editor.MaxLength;

			var currentControlText = nativeControl.Text;

			if (currentControlText.Length > editor.MaxLength)
				nativeControl.Text = currentControlText.Substring(0, editor.MaxLength);
		}

		public static void UpdateIsReadOnly(this TextBox nativeControl, IEditor editor)
		{
			nativeControl.IsReadOnly = editor.IsReadOnly;
		}
	}
}
