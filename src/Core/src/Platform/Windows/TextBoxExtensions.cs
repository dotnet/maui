using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Media;

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

		public static void UpdateCharacterSpacing(this MauiTextBox textBox, ITextStyle textStyle)
		{
			textBox.CharacterSpacing = textStyle.CharacterSpacing.ToEm();
		}

		public static void UpdateCharacterSpacing(this MauiTextBox textBox, IEntry entry)
		{
			textBox.CharacterSpacing = entry.CharacterSpacing.ToEm();
		}

		public static void UpdateReturnType(this MauiTextBox textBox, IEntry entry)
		{
			textBox.InputScope = entry.ReturnType.ToNative();
    	}

		public static void UpdateClearButtonVisibility(this MauiTextBox textBox, IEntry entry)
		{
			textBox.ClearButtonVisible = entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing;
		}

		public static void UpdatePlaceholder(this MauiTextBox textBox, IEditor editor)
		{
			textBox.PlaceholderText = editor.Placeholder ?? string.Empty;
		}

		public static void UpdatePlaceholder(this MauiTextBox textBox, IEntry entry)
		{
			textBox.PlaceholderText = entry.Placeholder ?? string.Empty;
		}
	
		public static void UpdatePlaceholderColor(this MauiTextBox textBox, IPlaceholder placeholder, Brush? defaultPlaceholderColorBrush, Brush? defaultPlaceholderColorFocusBrush)
		{
			Color placeholderColor = placeholder.PlaceholderColor;

			BrushHelpers.UpdateColor(placeholderColor, ref defaultPlaceholderColorBrush,
				() => textBox.PlaceholderForegroundBrush, brush => textBox.PlaceholderForegroundBrush = brush);

			BrushHelpers.UpdateColor(placeholderColor, ref defaultPlaceholderColorFocusBrush,
				() => textBox.PlaceholderForegroundFocusBrush, brush => textBox.PlaceholderForegroundFocusBrush = brush);
		}

		public static void UpdateFont(this MauiTextBox nativeControl, IText text, IFontManager fontManager) =>
			nativeControl.UpdateFont(text.Font, fontManager);

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
    
		public static void UpdateIsPassword(this MauiTextBox textBox, IEntry entry)
		{
			textBox.IsPassword = entry.IsPassword;
		}

		public static void UpdateIsTextPredictionEnabled(this MauiTextBox textBox, IEditor editor)
		{
			textBox.UpdateInputScope(editor);
		}

		public static void UpdateKeyboard(this MauiTextBox textBox, IEditor editor)
		{
			textBox.UpdateInputScope(editor);
		}

		internal static void UpdateInputScope(this MauiTextBox textBox, ITextInput textInput)
		{
			if (textInput.Keyboard is CustomKeyboard custom)
			{
				textBox.IsTextPredictionEnabled = (custom.Flags & KeyboardFlags.Suggestions) != 0;
				textBox.IsSpellCheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) != 0;
			}
			else
			{
				textBox.IsTextPredictionEnabled = textInput.IsTextPredictionEnabled;

				// TODO: Update IsSpellCheckEnabled
			}

			textBox.InputScope = textInput.Keyboard.ToInputScope();
		}

		public static void UpdateHorizontalTextAlignment(this MauiTextBox textBox, IEntry entry)
		{
			// We don't have a FlowDirection yet, so there's nothing to pass in here. 
			// TODO: Update this when FlowDirection is available 
			// (or update the extension to take an ILabel instead of an alignment and work it out from there) 
			textBox.TextAlignment = entry.HorizontalTextAlignment.ToNative(true);
		}

		public static void UpdateVerticalTextAlignment(this MauiTextBox textBox, IEntry entry)
		{
			textBox.VerticalAlignment = entry.VerticalTextAlignment.ToNativeVerticalAlignment();
    }
	}
}