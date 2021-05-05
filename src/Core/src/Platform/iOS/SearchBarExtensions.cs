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
	}
}