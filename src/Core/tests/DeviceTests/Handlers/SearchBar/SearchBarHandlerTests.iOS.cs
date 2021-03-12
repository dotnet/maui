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
	}
}