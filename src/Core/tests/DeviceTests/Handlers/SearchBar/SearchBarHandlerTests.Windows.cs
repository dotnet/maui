using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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

			// The cancel button won't exist in the SearchBar until the SearchBar is loaded (and OnApplyTemplate is called)
			// so we need to attach the SearchBar to the running app before we can check the color

			await AttachAndRun(searchBar, async (searchBarHandler) =>
			{
				await ValidatePropertyInitValue(searchBar, () => searchBar.CancelButtonColor, GetNativeCancelButtonColor, expected);
			});
		}

		static AutoSuggestBox GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			searchBarHandler.PlatformView;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Text;

		static string SetNativeText(SearchBarHandler searchBarHandler, string value) =>
			GetNativeSearchBar(searchBarHandler).Text = value;

		static int GetCursorStartPosition(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetNativeSearchBar(searchBarHandler);

			var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

			if (textBox is not null)
			{
				return textBox.GetCursorPosition();
			}

			return -1;
		}

		static void UpdateCursorStartPosition(SearchBarHandler searchBarHandler, int position)
		{
			var platformSearchBar = GetNativeSearchBar(searchBarHandler);

			var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

			textBox?.SelectionStart = position;
		}

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

		bool GetNativeIsSpellCheckEnabled(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetNativeSearchBar(searchBarHandler);

			var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

			if (textBox is not null)
			{
				return textBox.IsSpellCheckEnabled;
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