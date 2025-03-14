using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using Xunit;
using System.ComponentModel;

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

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a SearchBar should match with native Translation")]
		public async Task SearchBarTranslationConsistent()
		{
			var searchBar = new SearchBar()
			{
				Text = "SearchBar Test",
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var translation = nativeView.TranslationX;
				var density = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Density;
				var expectedInPixels = density * searchBar.TranslationX;

				Assert.Equal(expectedInPixels, translation, 1.0);

				var translationY = nativeView.TranslationY;
				var expectedYInPixels = density * searchBar.TranslationY;
				Assert.Equal(expectedYInPixels, translationY, 1.0);
			});
		}
	}
}
