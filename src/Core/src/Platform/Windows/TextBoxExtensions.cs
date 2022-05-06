#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public static class TextBoxExtensions
	{
		public static void UpdateIsPassword(this TextBox platformControl, IEntry entry)
		{
			if (platformControl is MauiPasswordTextBox passwordTextBox)
				passwordTextBox.IsPassword = entry.IsPassword;
		}

		public static void UpdateText(this TextBox platformControl, ITextInput textInput)
		{
			var newText = textInput.Text;

			if (platformControl is MauiPasswordTextBox passwordTextBox && passwordTextBox.Password == newText)
				return;
			if (platformControl.Text == newText)
				return;

			platformControl.Text = newText ?? string.Empty;

			if (!string.IsNullOrEmpty(platformControl.Text))
				platformControl.SelectionStart = platformControl.Text.Length;
		}

		static readonly string[] BackgroundColorKeys =
		{
			"TextControlBackground",
			"TextControlBackgroundPointerOver",
			"TextControlBackgroundFocused",
			"TextControlBackgroundDisabled"
		};

		public static void UpdateBackground(this TextBox textBox, IView view)
		{
			var brush = view.Background?.ToPlatform();

			if (brush == null)
			{
				textBox.Resources.RemoveKeys(BackgroundColorKeys);
				textBox.ClearValue(Control.BackgroundProperty);
			}
			else
			{
				textBox.Resources.SetValueForAllKey(BackgroundColorKeys, brush);
				textBox.Background = brush;
			}
		}
		
		static readonly string[] TextColorKeys =
		{
			"TextControlForeground",
			"TextControlForegroundPointerOver",
			"TextControlForegroundFocused",
			"TextControlForegroundDisabled"
		};

		public static void UpdateTextColor(this TextBox textBox, ITextStyle textStyle)
		{
			var brush = textStyle.TextColor?.ToPlatform();

			if (brush == null)
			{
				textBox.Resources.RemoveKeys(TextColorKeys);
				textBox.ClearValue(Control.ForegroundProperty);
			}
			else
			{
				textBox.Resources.SetValueForAllKey(TextColorKeys, brush);
				textBox.Foreground = brush;
			}
		}

		public static void UpdateCharacterSpacing(this TextBox textBox, ITextStyle textStyle)
		{
			textBox.CharacterSpacing = textStyle.CharacterSpacing.ToEm();
		}

		public static void UpdateReturnType(this TextBox textBox, ITextInput textInput)
		{
			textBox.UpdateInputScope(textInput);
		}

		public static void UpdateClearButtonVisibility(this TextBox textBox, IEntry entry) =>
			MauiTextBox.SetIsDeleteButtonEnabled(textBox, entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing);

		public static void UpdatePlaceholder(this TextBox textBox, IPlaceholder placeholder)
		{
			textBox.PlaceholderText = placeholder.Placeholder ?? string.Empty;
		}

		static readonly string[] PlaceholderColorKeys =
		{
			"TextControlPlaceholderForeground",
			"TextControlPlaceholderForegroundPointerOver",
			"TextControlPlaceholderForegroundFocused",
			"TextControlPlaceholderForegroundDisabled"
		};

		public static void UpdatePlaceholderColor(this TextBox textBox, IPlaceholder placeholder)
		{
			var brush = placeholder.PlaceholderColor?.ToPlatform();

			if (brush == null)
			{
				textBox.Resources.RemoveKeys(PlaceholderColorKeys);
				textBox.ClearValue(TextBox.PlaceholderForegroundProperty);
			}
			else
			{
				textBox.Resources.SetValueForAllKey(PlaceholderColorKeys, brush);
				textBox.PlaceholderForeground = brush;
			}
		}

		public static void UpdateFont(this TextBox platformControl, IText text, IFontManager fontManager) =>
			platformControl.UpdateFont(text.Font, fontManager);

		public static void UpdateIsReadOnly(this TextBox textBox, ITextInput textInput)
		{
			textBox.IsReadOnly = textInput.IsReadOnly;
		}

		public static void UpdateMaxLength(this TextBox textBox, ITextInput textInput)
		{
			var maxLength = textInput.MaxLength;

			if (maxLength == 0)
				textBox.IsReadOnly = true;
			else
				textBox.IsReadOnly = textInput.IsReadOnly;

			if (maxLength == -1)
				maxLength = int.MaxValue;

			textBox.MaxLength = maxLength;

			var currentControlText = textBox.Text;

			if (currentControlText.Length > maxLength)
				textBox.Text = currentControlText.Substring(0, maxLength);
		}

		public static void UpdateIsTextPredictionEnabled(this TextBox textBox, ITextInput textInput)
		{
			textBox.UpdateInputScope(textInput);
		}

		public static void UpdateKeyboard(this TextBox textBox, ITextInput textInput)
		{
			textBox.UpdateInputScope(textInput);
		}

		internal static void UpdateInputScope(this TextBox textBox, ITextInput textInput)
		{
			if (textInput.Keyboard is CustomKeyboard custom)
			{
				textBox.IsTextPredictionEnabled = (custom.Flags & KeyboardFlags.Suggestions) != 0;
				textBox.IsSpellCheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) != 0;
			}
			else
			{
				textBox.IsTextPredictionEnabled = textInput.IsTextPredictionEnabled;
				textBox.IsSpellCheckEnabled = textInput.IsTextPredictionEnabled;
			}

			var inputScope = new UI.Xaml.Input.InputScope();

			if (textInput is IEntry entry && entry.ReturnType == ReturnType.Search)
				inputScope.Names.Add(new UI.Xaml.Input.InputScopeName(UI.Xaml.Input.InputScopeNameValue.Search));

			inputScope.Names.Add(textInput.Keyboard.ToInputScopeName());

			textBox.InputScope = inputScope;
		}

		public static void UpdateHorizontalTextAlignment(this TextBox textBox, ITextAlignment textAlignment)
		{
			// We don't have a FlowDirection yet, so there's nothing to pass in here. 
			// TODO: Update this when FlowDirection is available 
			// (or update the extension to take an ILabel instead of an alignment and work it out from there) 
			textBox.TextAlignment = textAlignment.HorizontalTextAlignment.ToPlatform(true);
		}

		public static void UpdateVerticalTextAlignment(this TextBox textBox, ITextAlignment textAlignment) =>
			MauiTextBox.SetVerticalTextAlignment(textBox, textAlignment.VerticalTextAlignment.ToPlatformVerticalAlignment());

		public static void UpdateCursorPosition(this TextBox textBox, ITextInput entry)
		{
			if (textBox.SelectionStart != entry.CursorPosition)
				textBox.SelectionStart = entry.CursorPosition;
		}

		public static void UpdateSelectionLength(this TextBox textBox, ITextInput entry)
		{
			if (textBox.SelectionLength != entry.SelectionLength)
				textBox.SelectionLength = entry.SelectionLength;
		}
	}
}