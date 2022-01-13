using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Platform
{
	public static class EditorExtensions
	{
		public static void UpdateText(this Editor nativeEditor, IText editor)
		{
			if (nativeEditor.Text != editor.Text)
				nativeEditor.Text = editor.Text ?? "";
		}

		public static void UpdateTextColor(this Editor nativeEditor, ITextStyle editor)
		{
			nativeEditor.TextColor = editor.TextColor.ToNative();
		}

		public static void UpdateHorizontalTextAlignment(this Editor nativeEditor, ITextAlignment editor)
		{
			nativeEditor.HorizontalTextAlignment = editor.HorizontalTextAlignment.ToNative();
		}

		public static void UpdateFont(this Editor nativeEditor, ITextStyle textStyle, IFontManager fontManager)
		{
			nativeEditor.FontSize = textStyle.Font.Size > 0 ? textStyle.Font.Size.ToScaledPoint() : 25d.ToScaledPoint();
			nativeEditor.FontAttributes = textStyle.Font.GetFontAttributes();
			nativeEditor.FontFamily = fontManager.GetFontFamily(textStyle.Font.Family) ?? "";
		}

		public static void UpdatePlaceholder(this Editor nativeEditor, ITextInput editor)
		{
			nativeEditor.Placeholder = editor.Placeholder ?? string.Empty;
		}

		public static void UpdatePlaceholder(this Editor nativeEditor, string placeholder)
		{
			nativeEditor.Placeholder = placeholder;
		}

		public static void UpdatePlaceholderColor(this Editor nativeEditor, ITextInput editor)
		{
			nativeEditor.PlaceholderColor = editor.PlaceholderColor.ToNative();
		}

		public static void UpdatePlaceholderColor(this Editor nativeEditor, GColor color)
		{
			nativeEditor.PlaceholderColor = color.ToNative();
		}

		public static void UpdateIsReadOnly(this Editor nativeEditor, ITextInput editor)
		{
			nativeEditor.IsReadOnly = editor.IsReadOnly;
		}

		public static void UpdateIsTextPredictionEnabled(this Editor nativeEditor, ITextInput editor)
		{
			nativeEditor.IsTextPredictionEnabled = editor.IsTextPredictionEnabled;
		}

		public static void UpdateMaxLength(this Editor nativeEditor, ITextInput editor) =>
			nativeEditor.MaxLength = editor.MaxLength;

		public static void UpdateKeyboard(this Editor nativeEditor, ITextInput editor)
		{
			nativeEditor.Keyboard = editor.Keyboard.ToNative();
		}
	}
}
