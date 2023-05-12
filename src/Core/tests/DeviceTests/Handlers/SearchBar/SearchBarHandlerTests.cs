using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SearchBar)]
	public partial class SearchBarHandlerTests : CoreHandlerTestBase<SearchBarHandler, SearchBarStub>
	{
		[Theory(DisplayName = "Background Initializes Correctly"
#if IOS
			, Skip = "This test is currently invalid on iOS https://github.com/dotnet/maui/issues/13693"
#endif
			)]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BackgroundInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var searchBar = new SearchBarStub
			{
				Background = new SolidPaintStub(expected),
				Text = "Background",
			};

			await ValidateHasColor(searchBar, expected);
		}

		[Fact(DisplayName = "Text Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var searchBar = new SearchBarStub
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.Text, GetNativeText, searchBar.Text);
		}

		[Theory(DisplayName = "Query Text Updates Correctly")]
		[InlineData(null, null)]
		[InlineData(null, "Query")]
		[InlineData("Query", null)]
		[InlineData("Query", "Another search query")]
		public async Task TextUpdatesCorrectly(string setValue, string unsetValue)
		{
			var searchBar = new SearchBarStub();

			await ValidatePropertyUpdatesValue(
				searchBar,
				nameof(ISearchBar.Text),
				h =>
				{
					var n = GetNativeText(h);
					if (string.IsNullOrEmpty(n))
						n = null; // Native platforms may not support null text
					return n;
				},
				setValue,
				unsetValue);
		}

		[Fact(DisplayName = "TextColor Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var searchBar = new SearchBarStub
			{
				Text = "TextColor",
				TextColor = Colors.Red
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.TextColor, GetNativeTextColor, Colors.Red);
		}

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var searchBar = new SearchBarStub
			{
				Text = "TextColor",
				TextColor = null,
			};

			await CreateHandlerAsync(searchBar);
		}

		[Fact(DisplayName = "Placeholder Initializes Correctly")]
		public async Task PlaceholderInitializesCorrectly()
		{
			var searchBar = new SearchBarStub
			{
				Placeholder = "Placeholder"
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.Placeholder, GetNativePlaceholder, searchBar.Placeholder);
		}

		[Theory(DisplayName = "MaxLength Initializes Correctly")]
		[InlineData(2)]
		[InlineData(5)]
		[InlineData(8)]
		[InlineData(10)]
		public async Task MaxLengthInitializesCorrectly(int maxLength)
		{
			const string text = "Lorem ipsum dolor sit amet";
			var expectedText = text.Substring(0, maxLength);

			var searchBar = new SearchBarStub()
			{
				MaxLength = maxLength,
				Text = text
			};

			var platformText = await GetValueAsync(searchBar, GetNativeText);

			Assert.Equal(expectedText, platformText);
		}

		[Fact(DisplayName = "CancelButtonColor Initialize Correctly"
#if WINDOWS
			, Skip = "This test currently fails on Windows due to https://github.com/dotnet/maui/issues/13507"
#endif
			)]
		public async Task CancelButtonColorInitializeCorrectly()
		{
			var searchBar = new SearchBarStub()
			{
				Text = "Search",
				Width = 200,
				CancelButtonColor = Colors.Yellow,
			};

			await ValidateHasColor(searchBar, Colors.Yellow);
		}

		[Fact(DisplayName = "Null Cancel Button Color Doesn't Crash")]
		public async Task NullCancelButtonColorDoesntCrash()
		{
			var searchBar = new SearchBarStub
			{
				CancelButtonColor = null,
			};

			await CreateHandlerAsync(searchBar);
		}

		[Fact(DisplayName = "Default Input Field is at least 44dp high")]
		public async Task DefaultInputFieldIsAtLeast44DpHigh()
		{
			var searchBar = new SearchBarStub()
			{
				Text = "search bar text",
				Width = 200
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(searchBar);
				await AssertionExtensions.AttachAndRun(handler.PlatformView, () =>
				{
					var height = GetInputFieldHeight(handler);
					Assert.True(height >= 44);
				});
			});
		}

#if !WINDOWS
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsReadOnlyInitializesCorrectly(bool isReadOnly)
		{
			var searchBar = new SearchBarStub()
			{
				IsReadOnly = isReadOnly,
				Text = "Test"
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.IsReadOnly, GetNativeIsReadOnly, searchBar.IsReadOnly);
		}

		[Theory(DisplayName = "Validates Numeric Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), true)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateNumericKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var searchBar = new SearchBarStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(searchBar, () => expected, GetNativeIsNumericKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Email Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), true)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateEmailKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var searchBar = new SearchBarStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(searchBar, () => expected, GetNativeIsEmailKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Telephone Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), true)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateTelephoneKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var searchBar = new SearchBarStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(searchBar, () => expected, GetNativeIsTelephoneKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Url Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), true)]
		public async Task ValidateUrlKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var searchBar = new SearchBarStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(searchBar, () => expected, GetNativeIsUrlKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Text Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), true)]
		[InlineData(nameof(Keyboard.Url), false)]
#if WINDOWS
		// The Text keyboard is the default one on Windows
		[InlineData(nameof(Keyboard.Default), true)]
		// Plain is the same as the Default keyboard on Windows
		[InlineData(nameof(Keyboard.Plain), true)]
#else
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
#endif
		public async Task ValidateTextKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var searchBar = new SearchBarStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(searchBar, () => expected, GetNativeIsTextKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Chat Keyboard")]
		[InlineData(nameof(Keyboard.Chat), true)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateChatKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var searchBar = new SearchBarStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(searchBar, () => expected, GetNativeIsChatKeyboard, expected);
		}

		[Category(TestCategory.SearchBar)]
		public class SearchBarTextInputTests : TextInputHandlerTests<SearchBarHandler, SearchBarStub>
		{
			protected override void SetNativeText(SearchBarHandler searchBarHandler, string text)
			{
				SearchBarHandlerTests.SetNativeText(searchBarHandler, text);
			}

			protected override int GetCursorStartPosition(SearchBarHandler searchBarHandler)
			{
				return SearchBarHandlerTests.GetCursorStartPosition(searchBarHandler);
			}

			protected override void UpdateCursorStartPosition(SearchBarHandler searchBarHandler, int position)
			{
				SearchBarHandlerTests.UpdateCursorStartPosition(searchBarHandler, position);
			}
		}
#endif

		[Category(TestCategory.SearchBar)]
		public class SearchBarTextStyleTests : TextStyleHandlerTests<SearchBarHandler, SearchBarStub>
		{
		}
	}
}