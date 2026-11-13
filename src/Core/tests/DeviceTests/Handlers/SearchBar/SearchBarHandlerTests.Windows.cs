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
		// Regression tests for https://github.com/dotnet/maui/issues/30779
		// SearchBar.CursorPosition and SelectionLength were not updated when the user typed

		[Fact(DisplayName = "CursorPosition Updates After Setting Native Text (Issue 30779)")]
		public async Task CursorPositionUpdatesAfterSettingNativeText()
		{
			var searchBar = new SearchBarStub();
			int virtualViewCursorPosition = -1;

			await AttachAndRun(searchBar, async (handler) =>
			{
				await AssertEventually(() => handler.PlatformView.IsLoaded());

				// Simulate user typing by setting text on the native AutoSuggestBox
				GetNativeSearchBar(handler).Text = "Hello";

				// Wait for the SelectionChanged event to propagate back to the VirtualView.
				// On Windows the update is synchronous, but AssertEventually is used as a
				// safety net in case of any scheduling differences in the test environment.
				await AssertEventually(() => searchBar.CursorPosition == 5,
					message: "CursorPosition should update to 5 after setting native text to 'Hello'");

				virtualViewCursorPosition = searchBar.CursorPosition;
			});

			// After setting "Hello", cursor should be at position 5 (end of text),
			// not stuck at 0 as it was before the fix
			Assert.Equal(5, virtualViewCursorPosition);
		}

		[Fact(DisplayName = "SelectionLength Updates When Text Is Selected Natively (Issue 30779)")]
		public async Task SelectionLengthUpdatesWhenTextIsSelectedNatively()
		{
			var searchBar = new SearchBarStub { Text = "Hello World" };
			int virtualSelectionLength = -1;
			int virtualCursorPosition = -1;

			await AttachAndRun(searchBar, async (handler) =>
			{
				await AssertEventually(() => handler.PlatformView.IsLoaded());

				var platformSearchBar = GetNativeSearchBar(handler);
				var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

				// The inner TextBox must exist after the AutoSuggestBox is loaded
				Assert.NotNull(textBox);

				// Programmatically select the word "Hello" (chars 0–5)
				textBox.SelectionStart = 0;
				textBox.SelectionLength = 5;

				// Wait for the SelectionChanged event to propagate back to the VirtualView.
				// On Windows the update is synchronous, but AssertEventually is used as a
				// safety net in case of any scheduling differences in the test environment.
				await AssertEventually(() => searchBar.SelectionLength == 5,
					message: "SelectionLength should update to 5 after selecting text natively");

				virtualSelectionLength = searchBar.SelectionLength;
				virtualCursorPosition = searchBar.CursorPosition;
			});

			// Selection of 5 characters starting at position 0 should be reflected in the VirtualView
			Assert.Equal(0, virtualCursorPosition);
			Assert.Equal(5, virtualSelectionLength);
		}

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

			if (textBox is not null)
			{
				textBox.SelectionStart = position;
			}
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