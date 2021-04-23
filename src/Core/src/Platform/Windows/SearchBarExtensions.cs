using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdateText(this AutoSuggestBox autoSuggestBox, ISearchBar searchBar)
		{
			autoSuggestBox.Text = searchBar.Text;
		}
	}
}
