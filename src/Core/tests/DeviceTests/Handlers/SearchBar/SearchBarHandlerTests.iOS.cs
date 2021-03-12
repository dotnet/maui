using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
		UISearchBar GetNativeEntry(SearchBarHandler searchBarHandler) =>
			(UISearchBar)searchBarHandler.View;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeEntry(searchBarHandler).Text;

		string GetNativePlaceholder(SearchBarHandler searchBarHandler)
		{
			var searchBar = GetNativeEntry(searchBarHandler);
			var textField = searchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return string.Empty;

			return searchBar.Placeholder;
		}
	}
}