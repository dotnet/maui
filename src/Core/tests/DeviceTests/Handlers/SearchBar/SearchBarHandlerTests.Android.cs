using Microsoft.Maui.Handlers;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
		SearchView GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			(SearchView)searchBarHandler.View;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Query;

		string GetNativePlaceholder(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).QueryHint;
	}
}