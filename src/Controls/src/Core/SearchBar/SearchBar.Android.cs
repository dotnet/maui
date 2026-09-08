#nullable disable
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapText(SearchBarHandler handler, SearchBar searchBar) =>
			MapText((ISearchBarHandler)handler, searchBar);

		public static void MapText(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchViewExtensions.UpdateText(handler.PlatformView, searchBar);
		}

		// Material3 specific overload for SearchBarHandler2
		// TODO: Material3: Make it public in .NET 11
		internal static void MapText(SearchBarHandler2 handler, SearchBar searchBar)
		{
			if (handler.PlatformView?.EditText is null)
			{
				return;
			}

			Platform.EditTextExtensions.UpdateText(handler.PlatformView.EditText, searchBar);
		}
	}
}
