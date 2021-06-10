using UIKit;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdateText(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.Text = searchBar.Text;
		}

		public static void UpdatePlaceholder(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.Placeholder = searchBar.Placeholder;
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
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToNative(), UIControlState.Normal);
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToNative(), UIControlState.Highlighted);
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToNative(), UIControlState.Disabled);
			}
		}
	}
}