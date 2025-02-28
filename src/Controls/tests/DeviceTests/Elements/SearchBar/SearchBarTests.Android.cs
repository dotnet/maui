using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

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

		Task<float> GetPlatformOpacity(SearchBarHandler searchBarHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(searchBarHandler);
				return nativeView.Alpha;
			});
		}
		
		[Fact]
		[Description("The ScaleX property of a SearchBar should match with native ScaleX")]
        public async Task ScaleXConsistent()
        {
            var searchBar = new SearchBar() { ScaleX = 0.45f };
            var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
            var expected = searchBar.ScaleX;
            var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
            Assert.Equal(expected, platformScaleX);
        }

		[Fact]
		[Description("The ScaleY property of a SearchBar should match with native ScaleY")]
        public async Task ScaleYConsistent()
        {
            var searchBar = new SearchBar() { ScaleY = 0.45f };
            var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
            var expected = searchBar.ScaleY;
            var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
            Assert.Equal(expected, platformScaleY);
        }
	}
}
