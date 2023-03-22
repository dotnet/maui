using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
		[Theory(DisplayName = "CancelButtonColor Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task CancelButtonColorInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var searchBar = new SearchBarStub()
			{
				Text = "Test",
				CancelButtonColor = expected
			};

			searchBar.Focus();

			await InvokeOnMainThreadAsync(async () =>
			{
				var searchBarHandler = CreateHandler(searchBar);
				await searchBarHandler.PlatformView.AttachAndRun(async () =>
				{
					await AssertionExtensions.Wait(() => searchBarHandler.PlatformView.FocusState != UI.Xaml.FocusState.Unfocused);
				});
			});

			await ValidatePropertyInitValue(searchBar, () => searchBar.CancelButtonColor, GetNativeCancelButtonColor, expected);
		}

		[Theory(DisplayName = "Is Text Prediction Enabled")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsTextPredictionEnabledCorrectly(bool isEnabled)
		{
			var searchBar = new SearchBarStub()
			{
				IsTextPredictionEnabled = isEnabled
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var searchBarHandler = CreateHandler(searchBar);
				await searchBarHandler.PlatformView.AttachAndRun(async () =>
				{
					await AssertionExtensions.Wait(() => searchBarHandler.PlatformView.Width != 0);
				});
			});

			var nativeIsTextPredictionEnabled = await GetValueAsync(searchBar, GetNativeIsTextPredictionEnabled);
			Assert.Equal(isEnabled, nativeIsTextPredictionEnabled);
		}

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

		double GetInputFieldHeight(SearchBarHandler searchBarHandler)
		{
			return GetNativeSearchBar(searchBarHandler).ActualHeight;
		}

		bool GetNativeIsTextPredictionEnabled(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetNativeSearchBar(searchBarHandler);

			var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

			if (textBox is not null)
			{
				return textBox.IsTextPredictionEnabled;
			}

			return false;
		}

		Color GetNativeCancelButtonColor(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetNativeSearchBar(searchBarHandler);
			var cancelButton = platformSearchBar.GetDescendantByName<Button>("DeleteButton");

			if (cancelButton is not null)
			{
				var foreground = cancelButton.Foreground;

				if (foreground is SolidColorBrush solidColorBrush)
					return solidColorBrush.Color.ToColor();
			}

			return Colors.Transparent;
		}
	}
}