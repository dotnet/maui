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
			var expected = searchBar.ScaleX;
			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var platformSearchBar = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformSearchBar.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a SearchBar should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var searchBar = new SearchBar() { ScaleY = 1.23f };
			var expected = searchBar.ScaleY;
			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var platformSearchBar = GetPlatformControl(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformSearchBar.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a SearchBar should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var searchBar = new SearchBar() { Scale = 2.0f };
			var expected = searchBar.Scale;
			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var platformSearchBar = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformSearchBar.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformSearchBar.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a SearchBar should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var searchBar = new SearchBar() { RotationX = 33.0 };
			var expected = searchBar.RotationX;
			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var platformSearchBar = GetPlatformControl(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => platformSearchBar.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a SearchBar should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var searchBar = new SearchBar() { RotationY = 87.0 };
			var expected = searchBar.RotationY;
			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var platformSearchBar = GetPlatformControl(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => platformSearchBar.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a SearchBar should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var searchBar = new SearchBar() { Rotation = 23.0 };
			var expected = searchBar.Rotation;
			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var platformSearchBar = GetPlatformControl(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => platformSearchBar.Rotation);
			Assert.Equal(expected, platformRotation);
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

		Task<bool> GetPlatformIsVisible(SearchBarHandler searchBarHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(searchBarHandler);
				return nativeView.Visibility == Android.Views.ViewStates.Visible;
			});
		}
	}
}
