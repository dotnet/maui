namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapIsSpellCheckEnabled(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.AutoSuggestBoxExtensions.UpdateIsSpellCheckEnabled(handler.PlatformView, searchBar);
		}

		public static void MapText(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.AutoSuggestBoxExtensions.UpdateText(handler.PlatformView, searchBar);
		}
	}
}
