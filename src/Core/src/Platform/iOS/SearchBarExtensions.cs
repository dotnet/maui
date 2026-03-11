using System;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SearchBarExtensions
	{
		internal static UITextField? GetSearchTextField(this UISearchBar searchBar)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13))
			{
				return searchBar.SearchTextField;
			}

			// Search Subviews up to two levels deep
			// https://stackoverflow.com/a/58056700
			foreach (var child in searchBar.Subviews)
			{
				if (child is UITextField childTextField)
					return childTextField;

				foreach (var grandChild in child.Subviews)
				{
					if (grandChild is UITextField grandChildTextField)
						return grandChildTextField;
				}
			}

			return null;
		}

		internal static void UpdateBackground(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var background = searchBar.Background;

			if (background is SolidPaint solidPaint)
				uiSearchBar.BarTintColor = solidPaint.Color.ToPlatform();

			if (background is GradientPaint gradientPaint)
				ViewExtensions.UpdateBackground(uiSearchBar, gradientPaint);

			if (background == null)
				uiSearchBar.BarTintColor = UISearchBar.Appearance.BarTintColor;
		}

		public static void UpdateIsEnabled(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.UserInteractionEnabled = searchBar.IsEnabled;
		}

		public static void UpdateText(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.Text = searchBar.Text;
		}

		public static void UpdatePlaceholder(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			var placeholder = searchBar.Placeholder ?? string.Empty;
			var placeholderColor = searchBar.PlaceholderColor is Color color ? color.ToPlatform() : ColorExtensions.PlaceholderColor;
			textField.AttributedPlaceholder = new NSAttributedString(str: placeholder, foregroundColor: placeholderColor);
			textField.AttributedPlaceholder.WithCharacterSpacing(searchBar.CharacterSpacing);
		}

		public static void UpdateFont(this UISearchBar uiSearchBar, ITextStyle textStyle, IFontManager fontManager)
		{
			uiSearchBar.UpdateFont(textStyle, fontManager, null);
		}

		public static void UpdateFont(this UISearchBar uiSearchBar, ITextStyle textStyle, IFontManager fontManager, UITextField? textField)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			textField.UpdateFont(textStyle, fontManager);
		}

		public static void UpdateVerticalTextAlignment(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.UpdateVerticalTextAlignment(searchBar, null);
		}

		public static void UpdateVerticalTextAlignment(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			textField.VerticalAlignment = searchBar.VerticalTextAlignment.ToPlatformVertical();
		}

		public static void UpdateMaxLength(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var maxLength = searchBar.MaxLength;

			if (maxLength == -1)
				maxLength = int.MaxValue;

			var currentControlText = uiSearchBar.Text;

			if (currentControlText?.Length > maxLength)
				uiSearchBar.Text = currentControlText.Substring(0, maxLength);
		}

		public static void UpdateIsReadOnly(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.UserInteractionEnabled = !(searchBar.IsReadOnly || searchBar.InputTransparent);
		}

		internal static bool ShouldShowCancelButton(this ISearchBar searchBar) =>
			!string.IsNullOrEmpty(searchBar.Text);

		public static void UpdateCancelButton(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.ShowsCancelButton = searchBar.ShouldShowCancelButton();

			// We can't cache the cancel button reference because iOS drops it when it's not displayed
			// and creates a brand new one when necessary, so we have to look for it each time
			var cancelButton = uiSearchBar.FindDescendantView<UIButton>();

			if (cancelButton == null)
				return;

			if (searchBar.CancelButtonColor != null)
			{
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Normal);
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Highlighted);
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Disabled);

				if (cancelButton.TraitCollection.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac)
					cancelButton.TintColor = searchBar.CancelButtonColor.ToPlatform();
			}
		}

		internal static void UpdateSearchIcon(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField?.LeftView is not UIImageView iconView || iconView.Image is null)
				return;

			if (searchBar.SearchIconColor is not null)
			{
				iconView.Image = iconView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
				iconView.TintColor = searchBar.SearchIconColor.ToPlatform();
			}

		}

		public static void UpdateIsTextPredictionEnabled(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField = null)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			if (searchBar.IsTextPredictionEnabled)
				textField.AutocorrectionType = UITextAutocorrectionType.Yes;
			else
				textField.AutocorrectionType = UITextAutocorrectionType.No;
		}

		public static void UpdateIsSpellCheckEnabled(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField = null)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			if (searchBar.IsSpellCheckEnabled)
				textField.SpellCheckingType = UITextSpellCheckingType.Yes;
			else
				textField.SpellCheckingType = UITextSpellCheckingType.No;
		}

		public static void UpdateKeyboard(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var keyboard = searchBar.Keyboard;

			uiSearchBar.ApplyKeyboard(keyboard);

			if (keyboard is not CustomKeyboard)
			{
				uiSearchBar.UpdateIsTextPredictionEnabled(searchBar);
				uiSearchBar.UpdateIsSpellCheckEnabled(searchBar);
			}

			uiSearchBar.ReloadInputViews();
		}

		public static void UpdateReturnType(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.ReturnKeyType = searchBar.ReturnType.ToPlatform();
		}

		internal static void UpdateCursorPosition(this UITextField textField, ISearchBar searchBar)
		{
			var selectedTextRange = textField.SelectedTextRange;
			if (selectedTextRange is null)
			{
				return;
			}

			if (textField.GetOffsetFromPosition(textField.BeginningOfDocument, selectedTextRange.Start) != searchBar.CursorPosition)
			{
				UpdateCursorSelection(textField, searchBar);
			}
		}

		internal static void UpdateSelectionLength(this UITextField textField, ISearchBar searchBar)
		{
			var selectedTextRange = textField.SelectedTextRange;
			if (selectedTextRange is null)
			{
				return;
			}

			if (textField.GetOffsetFromPosition(selectedTextRange.Start, selectedTextRange.End) != searchBar.SelectionLength)
			{
				UpdateCursorSelection(textField, searchBar);
			}
		}

		// Updates both the ISearchBar.CursorPosition and ISearchBar.SelectionLength properties.

		static void UpdateCursorSelection(this UITextField textField, ISearchBar searchBar)
		{
			if (searchBar.IsReadOnly)
			{
				return;
			}

			// Capture current values to avoid reading stale values after async dispatch
			int cursorPosition = searchBar.CursorPosition;
			int selectionLength = searchBar.SelectionLength;

			void UpdateSelection()
			{
				if (textField is not null && textField.Handle != IntPtr.Zero)
				{
					UITextPosition start = GetSelectionStart(textField, cursorPosition, out int startOffset);
					UITextPosition end = GetSelectionEnd(textField, start, startOffset, selectionLength);
					textField.SelectedTextRange = textField.GetTextRange(start, end);
				}
			}

			if (searchBar.IsFocused)
			{
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(UpdateSelection);
			}
			else
			{
				UpdateSelection();
			}
		}

		static UITextPosition GetSelectionStart(this UITextField textField, int cursorPosition, out int startOffset)
		{
			int textLength = textField.Text?.Length ?? 0;

			startOffset = Math.Max(0, Math.Min(textLength, cursorPosition));
			return textField.GetPosition(textField.BeginningOfDocument, startOffset) ?? textField.BeginningOfDocument;
		}

		static UITextPosition GetSelectionEnd(UITextField textField, UITextPosition start, int startOffset, int selectionLength)
		{
			int textLength = textField.Text?.Length ?? 0;
			int endOffset = Math.Max(startOffset, Math.Min(textLength, startOffset + selectionLength));
			var end = textField.GetPosition(start, endOffset - startOffset);
			return end ?? start;
		}
	}
}