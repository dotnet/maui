using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdateText(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.SetQuery(searchBar.Text, false);
		}

		public static void UpdatePlaceholder(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.QueryHint = searchBar.Placeholder;
		}
	}
}
