#nullable disable

using System;

namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapText(SearchBarHandler handler, SearchBar searchBar) =>
			MapText((ISearchBarHandler)handler, searchBar);

		[Obsolete("Use the SearchBarHandler's mapper instead")]
		public static void MapIsSpellCheckEnabled(SearchBarHandler handler, SearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsSpellCheckEnabled(searchBar);
		}

		[Obsolete("Use the SearchBarHandler's mapper instead.")]
		public static void MapIsSpellCheckEnabled(ISearchBarHandler handler, SearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsSpellCheckEnabled(searchBar);
		}

		public static void MapText(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.AutoSuggestBoxExtensions.UpdateText(handler.PlatformView, searchBar);
		}
	}
}