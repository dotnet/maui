using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using System.ComponentModel;
using Xunit;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarTests
	{
		static SearchView GetPlatformControl(SearchBarHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(SearchBarHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Query);
		}

		static int GetPlatformSelectionLength(SearchBarHandler searchBarHandler)
		{
			var control = GetPlatformControl(searchBarHandler);
			var editText = control.GetChildrenOfType<EditText>().FirstOrDefault();
			return editText.SelectionEnd - editText.SelectionStart;
		}

		static int GetPlatformCursorPosition(SearchBarHandler searchBarHandler)
		{
			var control = GetPlatformControl(searchBarHandler);
			var editText = control.GetChildrenOfType<EditText>().FirstOrDefault();
			return editText.SelectionStart;
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
				var isEnabled = nativeView.Enabled;

				Assert.Equal(expectedValue, isEnabled);
			});		
		}
	}
}
