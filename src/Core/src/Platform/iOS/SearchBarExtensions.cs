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
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return;

			textField.Text = searchBar.Placeholder;
		}
	}
}