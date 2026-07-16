#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapText(SearchBarHandler handler, SearchBar searchBar) =>
			MapText((ISearchBarHandler)handler, searchBar);

		public static void MapSearchBarStyle(SearchBarHandler handler, SearchBar searchBar) =>
			MapSearchBarStyle((ISearchBarHandler)handler, searchBar);

		public static void MapSearchBarStyle(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchBarExtensions.UpdateSearchBarStyle(handler.PlatformView, searchBar);
		}

		public static void MapText(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchBarExtensions.UpdateText(handler.PlatformView, searchBar);

			// Any text update requires that we update any attributed string formatting.
			// This must run even during handler connection because the SearchBarHandler mapper
			// applies CharacterSpacing/MaxLength/HorizontalTextAlignment before Text, so
			// UpdateText can wipe the attributed string that those mappers already set.
			SearchBarHandler.MapFormatting(handler, searchBar);
		}

		internal static void MapUserInteraction(ISearchBarHandler handler, SearchBar searchBar)
		{
			handler.PlatformView.UserInteractionEnabled = searchBar.IsEnabled && !searchBar.IsReadOnly && !searchBar.InputTransparent;
		}
	}
}
