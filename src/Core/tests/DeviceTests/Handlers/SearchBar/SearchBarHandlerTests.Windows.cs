using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
		static AutoSuggestBox GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			searchBarHandler.PlatformView;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Text;

		Color GetNativeTextColor(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetNativeSearchBar(searchBarHandler);
			var foreground = platformSearchBar.Foreground;

			if (foreground is SolidColorBrush solidColorBrush)
				return solidColorBrush.Color.ToColor();

			return Colors.Transparent;
		}

		string GetNativePlaceholder(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).PlaceholderText;

		Task ValidateHasColor(ISearchBar searchBar, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeSearchBar = GetNativeSearchBar(CreateHandler(searchBar));
				action?.Invoke();
				nativeSearchBar.AssertContainsColor(color);
			});
		}

		double GetInputFieldHeight(SearchBarHandler searchBarHandler)
		{
			return GetNativeSearchBar(searchBarHandler).ActualHeight;
		}
	}
}