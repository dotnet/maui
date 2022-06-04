namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapSearchBarStyle(SearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchBarExtensions.UpdateSearchBarStyle(handler.PlatformView, searchBar);
		}

		public static void MapText(SearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchBarExtensions.UpdateText(handler.PlatformView, searchBar);
		}
	}
}
