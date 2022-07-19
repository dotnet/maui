namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapIsSpellCheckEnabled(SearchBarHandler handler, SearchBar searchBar)
		{
			Platform.AutoSuggestBoxExtensions.UpdateIsSpellCheckEnabled(handler.PlatformView, searchBar);
		}

		public static void MapText(SearchBarHandler handler, SearchBar searchBar)
		{
			Platform.AutoSuggestBoxExtensions.UpdateText(handler.PlatformView, searchBar);
		}
	}
}
