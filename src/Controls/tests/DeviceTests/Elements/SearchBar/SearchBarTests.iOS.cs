using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarTests
	{
		static MauiSearchBar GetPlatformControl(SearchBarHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(SearchBarHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		static int GetPlatformSelectionLength(SearchBarHandler searchBarHandler)
		{
			var control = searchBarHandler.QueryEditor;
			return control.GetSelectedTextLength();
		}

		static int GetPlatformCursorPosition(SearchBarHandler searchBarHandler)
		{
			var control = searchBarHandler.QueryEditor;
			return control.GetCursorPosition();
		}
	}
}
