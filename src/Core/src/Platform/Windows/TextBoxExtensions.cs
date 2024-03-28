#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Platform
{
	public static class TextBoxExtensions
	{
		public static void UpdateIsPassword(this TextBox platformControl, IEntry entry)
		{
			if (platformControl is MauiPasswordTextBox passwordTextBox)
				MauiPasswordTextBox.IsPassword = entry.IsPassword;
		}

		public static void UpdateText(this TextBox platformControl, ITextInput textInput)
		{
			var newText = textInput.Text;

			if (platformControl is MauiPasswordTextBox passwordTextBox && MauiPasswordTextBox.Password == newText)
				return;

			if (platformControl.Text == newText)
				return;

			platformControl.Text = newText ?? string.Empty;

			if (!string.IsNullOrEmpty(platformControl.Text))
				platformControl.Select(platformControl.Text.Length, 0);
		}

		public static void UpdateBackground(this TextBox textBox, IView view)
		{
			var brush = view.Background?.ToPlatform();

			if (brush is null)
				textBox.Resources.RemoveKeys(BackgroundResourceKeys);
			else
				textBox.Resources.SetValueForAllKey(BackgroundResourceKeys, brush);

			textBox.RefreshThemeResources();
		}

		static readonly string[] BackgroundResourceKeys =
		{
			"TextControlBackground",
			"TextControlBackgroundPointerOver",
			"TextControlBackgroundFocused",
			"TextControlBackgroundDisabled",
		};

		public static void UpdateTextColor(this TextBox textBox, ITextStyle textStyle)
		{
			var brush = textStyle.TextColor?.ToPlatform();

			if (brush is null)
				textBox.Resources.RemoveKeys(TextColorResourceKeys);
			else
				textBox.Resources.SetValueForAllKey(TextColorResourceKeys, brush);

			textBox.RefreshThemeResources();
		}

		static readonly string[] TextColorResourceKeys =
		{
			"TextControlForeground",
			"TextControlForegroundPointerOver",
			"TextControlForegroundFocused",
			"TextControlForegroundDisabled",
		};

		public static void UpdateCharacterSpacing(this TextBox textBox, ITextStyle textStyle)
		{
			textBox.CharacterSpacing = textStyle.CharacterSpacing.ToEm();
		}

		public static void UpdateReturnType(this TextBox textBox, ITextInput textInput)
		{
			textBox.UpdateInputScope(textInput);
		}

		internal static bool GetClearButtonVisibility(this TextBox textBox) =>
			MauiTextBox.GetIsDeleteButtonEnabled(textBox);

		public static void UpdateClearButtonVisibility(this TextBox textBox, IEntry entry) =>
			MauiTextBox.SetIsDeleteButtonEnabled(textBox, entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing);

		public static void UpdatePlaceholder(this TextBox textBox, IPlaceholder placeholder)
		{
			textBox.PlaceholderText = placeholder.Placeholder ?? string.Empty;
		}

		public static void UpdatePlaceholderColor(this TextBox textBox, IPlaceholder placeholder)
		{
			var brush = placeholder.PlaceholderColor?.ToPlatform();

			if (brush is null)
			{
				// Windows.Foundation.UniversalApiContract < 5
				textBox.Resources.RemoveKeys(PlaceholderColorResourceKeys);
				// Windows.Foundation.UniversalApiContract >= 5
				textBox.ClearValue(TextBox.PlaceholderForegroundProperty);
			}
			else
			{
				// Windows.Foundation.UniversalApiContract < 5
				textBox.Resources.SetValueForAllKey(PlaceholderColorResourceKeys, brush);
				// Windows.Foundation.UniversalApiContract >= 5
				textBox.PlaceholderForeground = brush;
			}

			textBox.RefreshThemeResources();
		}

		static readonly string[] PlaceholderColorResourceKeys =
		{
			"TextControlPlaceholderForeground",
			"TextControlPlaceholderForegroundPointerOver",
			"TextControlPlaceholderForegroundFocused",
			"TextControlPlaceholderForegroundDisabled",
		};

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

		public static void UpdateIsSpellCheckEnabled(this TextBox textBox, ITextInput textInput)
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
				textBox.IsSpellCheckEnabled = textInput.IsSpellCheckEnabled;
			}

			var inputScope = new InputScope();

			if (textInput is IEntry entry && entry.ReturnType == ReturnType.Search)
				inputScope.Names.Add(new InputScopeName(InputScopeNameValue.Search));

			inputScope.Names.Add(textInput.Keyboard?.ToInputScopeName() ?? new InputScopeName(InputScopeNameValue.Default));

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
			// It seems that the TextBox does not limit the CursorPosition to the Text.Length natively
			entry.CursorPosition = Math.Min(entry.CursorPosition, textBox.Text.Length);

			if (textBox.SelectionStart != entry.CursorPosition)
				textBox.SelectionStart = entry.CursorPosition;
		}

		public static void UpdateSelectionLength(this TextBox textBox, ITextInput entry)
		{
			// It seems that the TextBox does not limit the SelectionLength to the Text.Length natively
			entry.SelectionLength = Math.Min(entry.SelectionLength, textBox.Text.Length - textBox.SelectionStart);

			if (textBox.SelectionLength != entry.SelectionLength)
				textBox.SelectionLength = entry.SelectionLength;
		}

		// TODO: NET8 issoto - Revisit this, marking this method as `internal` to avoid breaking public API changes
		internal static int GetCursorPosition(this TextBox textBox, int cursorOffset = 0)
		{
			var newCursorPosition = textBox.SelectionStart + cursorOffset;
			return Math.Max(0, newCursorPosition);
		}
	}
}