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

		public static void UpdateFont(this UISearchBar uiSearchBar, ISearchBar searchBar, IFontManager fontManager)
		{
			uiSearchBar.UpdateFont(searchBar, fontManager, null);
		}

		public static void UpdateFont(this UISearchBar uiSearchBar, ISearchBar searchBar, IFontManager fontManager, UITextField? textField)
		{
			textField ??= uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return;

			var uiFont = fontManager.GetFont(searchBar.Font);
			textField.Font = uiFont;
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

			textField.VerticalAlignment = searchBar.VerticalTextAlignment.ToNative();
		}
	}
}