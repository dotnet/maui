#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarTests
	{
		static AutoSuggestBox GetPlatformControl(SearchBarHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(SearchBarHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		static int GetPlatformSelectionLength(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetPlatformControl(searchBarHandler);

			var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

			if (textBox is not null)
			{
				return textBox.SelectionLength;
			}

			return -1;
		}

		static int GetPlatformCursorPosition(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetPlatformControl(searchBarHandler);

			var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

			if (textBox is not null)
			{
				return textBox.SelectionStart;
			}

			return -1;
		}

		Task<float> GetPlatformOpacity(SearchBarHandler searchBarHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(searchBarHandler);
				return (float)nativeView.Opacity;
			});
		}
	}
}
