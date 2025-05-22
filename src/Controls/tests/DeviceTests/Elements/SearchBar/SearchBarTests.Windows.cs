#nullable enable
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;

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

		[Fact]
		[Description("The IsEnabled of a SearchBar should match with native IsEnabled")]
		public async Task VerifySearchBarIsEnabledProperty()
		{
			var searchBar = new SearchBar
			{
				IsEnabled = false
			};
			var expectedValue = searchBar.IsEnabled;

			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.IsEnabled;
				Assert.Equal(expectedValue, isEnabled);
			});
		}

		Task<bool> GetPlatformIsVisible(SearchBarHandler searchBarHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(searchBarHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}
	}
}
