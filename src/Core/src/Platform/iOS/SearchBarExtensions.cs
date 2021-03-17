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

		//public static void UpdateHorizontalTextAlignment(this UISearchBar uiSearchBar, ISearchBar searchBar)
		//{
		//	UpdateHorizontalTextAlignment(uiSearchBar, searchBar, null);
		//}

		//public static void UpdateHorizontalTextAlignment(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField)
		//{
		//	textField ??= uiSearchBar.FindDescendantView<UITextField>();

		//	if (textField == null)
		//		return;

		//	// We don't have a FlowDirection yet, so there's nothing to pass in here. 
		//	// TODO: Update this when FlowDirection is available 
		//	// (or update the extension to take an ILabel instead of an alignment and work it out from there) 
		//	textField.TextAlignment = searchBar.HorizontalTextAlignment.ToNative(true);
		//}
	}
}