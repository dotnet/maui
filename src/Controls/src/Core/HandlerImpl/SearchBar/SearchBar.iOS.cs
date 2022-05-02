namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapText(SearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchBarExtensions.UpdateText(handler.PlatformView, searchBar);
		}
	}
}
