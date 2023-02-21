using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Platform
{
	public static class EditorExtensions
	{
		public static void UpdateText(this Editor platformEditor, IText editor)
		{
			if (platformEditor.Text != editor.Text)
				platformEditor.Text = editor.Text ?? "";
		}

		public static void UpdateTextColor(this Editor platformEditor, ITextStyle editor)
		{
			platformEditor.TextColor = editor.TextColor.ToPlatform();
		}

		public static void UpdateHorizontalTextAlignment(this Editor platformEditor, ITextAlignment editor)
		{
			platformEditor.HorizontalTextAlignment = editor.HorizontalTextAlignment.ToPlatform();
		}

		public static void UpdateVerticalTextAlignment(this Editor platformEditor, ITextAlignment editor)
		{
			switch (editor.HorizontalTextAlignment)
			{
				case TextAlignment.Start:
					platformEditor.VerticalAlignment = Tizen.NUI.VerticalAlignment.Top;
					break;
				case TextAlignment.Center:
					platformEditor.VerticalAlignment = Tizen.NUI.VerticalAlignment.Center;
					break;
				case TextAlignment.End:
					platformEditor.VerticalAlignment = Tizen.NUI.VerticalAlignment.Bottom;
					break;
			}
		}

		public static void UpdateFont(this Editor platformEditor, ITextStyle textStyle, IFontManager fontManager)
		{
			platformEditor.FontSize = textStyle.Font.Size > 0 ? textStyle.Font.Size.ToScaledPoint() : 25d.ToScaledPoint();
			platformEditor.FontAttributes = textStyle.Font.GetFontAttributes();
			platformEditor.FontFamily = fontManager.GetFontFamily(textStyle.Font.Family) ?? "";
		}

		public static void UpdatePlaceholder(this Editor platformEditor, ITextInput editor)
		{
			platformEditor.Placeholder = editor.Placeholder ?? string.Empty;
		}

		public static void UpdatePlaceholder(this Editor platformEditor, string placeholder)
		{
			platformEditor.Placeholder = placeholder;
		}

		public static void UpdatePlaceholderColor(this Editor platformEditor, ITextInput editor)
		{
			platformEditor.PlaceholderColor = editor.PlaceholderColor.ToPlatform();
		}

		public static void UpdatePlaceholderColor(this Editor platformEditor, GColor color)
		{
			platformEditor.PlaceholderColor = color.ToPlatform();
		}

		public static void UpdateIsReadOnly(this Editor platformEditor, ITextInput editor)
		{
			platformEditor.IsReadOnly = editor.IsReadOnly;
		}

		public static void UpdateIsTextPredictionEnabled(this Editor platformEditor, ITextInput editor)
		{
			platformEditor.IsTextPredictionEnabled = editor.IsTextPredictionEnabled;
		}

		public static void UpdateMaxLength(this Editor platformEditor, ITextInput editor) =>
			platformEditor.MaxLength = editor.MaxLength;

		public static void UpdateCursorPosition(this Editor platformEditor, ITextInput entry)
		{
			platformEditor.PrimaryCursorPosition = entry.CursorPosition;
		}

		public static void UpdateSelectionLength(this Editor platformEditor, ITextInput entry)
		{
			if (entry.SelectionLength == 0)
			{
				platformEditor.SelectNone();
			}
			else
			{
				platformEditor.SelectText(entry.CursorPosition, entry.CursorPosition + entry.SelectionLength);
			}
		}

		public static void UpdateKeyboard(this Editor platformEditor, ITextInput editor)
		{
			platformEditor.Keyboard = editor.Keyboard.ToPlatform();
		}

		public static void UpdateCharacterSpacing(this Editor platformEditor, ITextInput editor)
		{
			platformEditor.CharacterSpacing = editor.CharacterSpacing.ToScaledPixel();
		}
	}
}
