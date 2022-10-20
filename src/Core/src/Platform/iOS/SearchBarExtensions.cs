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
				return searchBar.SearchTextField;
			else
				return searchBar.GetSearchTextField();
		}

		// TODO: NET8 maybe make this public?
		internal static void UpdateBackground(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var background = searchBar.Background;

			if (background is SolidPaint solidPaint)
				uiSearchBar.BarTintColor = solidPaint.Color.ToPlatform();

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
			var placeholderColor = searchBar.PlaceholderColor;
			var foregroundColor = placeholderColor ?? ColorExtensions.PlaceholderColor.ToColor();

			textField.AttributedPlaceholder = foregroundColor == null
				? new NSAttributedString(placeholder)
				: new NSAttributedString(str: placeholder, foregroundColor: foregroundColor.ToPlatform());

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
			}
		}

		public static void UpdateIsTextPredictionEnabled(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			if (searchBar.IsTextPredictionEnabled)
				textField.AutocorrectionType = UITextAutocorrectionType.Yes;
			else
				textField.AutocorrectionType = UITextAutocorrectionType.No;
		}
	}
}