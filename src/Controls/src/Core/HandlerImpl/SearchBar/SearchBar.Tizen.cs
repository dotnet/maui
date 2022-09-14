namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapText(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView.Entry, searchBar);
		}
	}
}
