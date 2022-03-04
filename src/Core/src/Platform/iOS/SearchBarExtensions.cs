using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SearchBarExtensions
	{
		public static void UpdateText(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.Text = searchBar.Text;
		}

		public static void UpdatePlaceholder(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField)
		{
			textField ??= uiSearchBar.FindDescendantView<UITextField>();

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
			textField ??= uiSearchBar.FindDescendantView<UITextField>();

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
			textField ??= uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return;

			textField.VerticalAlignment = searchBar.VerticalTextAlignment.ToPlatform();
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

		public static void UpdateCancelButton(this UISearchBar uiSearchBar, ISearchBar searchBar,
			UIColor? cancelButtonTextColorDefaultNormal, UIColor? cancelButtonTextColorDefaultHighlighted, UIColor? cancelButtonTextColorDefaultDisabled)
		{
			uiSearchBar.ShowsCancelButton = !string.IsNullOrEmpty(uiSearchBar.Text);

			// We can't cache the cancel button reference because iOS drops it when it's not displayed
			// and creates a brand new one when necessary, so we have to look for it each time
			var cancelButton = uiSearchBar.FindDescendantView<UIButton>();

			if (cancelButton == null)
				return;

			if (searchBar.CancelButtonColor == null)
			{
				cancelButton.SetTitleColor(cancelButtonTextColorDefaultNormal, UIControlState.Normal);
				cancelButton.SetTitleColor(cancelButtonTextColorDefaultHighlighted, UIControlState.Highlighted);
				cancelButton.SetTitleColor(cancelButtonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Normal);
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Highlighted);
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Disabled);
			}
		}

		public static void UpdateIsTextPredictionEnabled(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField)
		{
			textField ??= uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return;

			if (searchBar.IsTextPredictionEnabled)
				textField.AutocorrectionType = UITextAutocorrectionType.Yes;
			else
				textField.AutocorrectionType = UITextAutocorrectionType.No;
		}
	}
}