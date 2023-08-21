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

		[Fact(DisplayName = "Text Property Initializes Correctly when Keyboard Mapper is Executed Before Text Mapper")]
		public async Task TextInitializesCorrectlyWhenKeyboardIsBeforeText()
		{
			var searchBar = new SearchBarStub()
			{
				Text = "Test Text Here"
			};

			CustomSearchBarHandler.TestMapper = new PropertyMapper<ISearchBar, ISearchBarHandler>(SearchBarHandler.Mapper)
			{
				// this mapper is run first and then the ones in the ctor arg (SearchBarHandler.Mapper)
				[nameof(ISearchBar.Keyboard)] = SearchBarHandler.MapKeyboard
			};

			await ValidatePropertyInitValue<string, CustomSearchBarHandler>(searchBar, () => searchBar.Text, GetNativeText, searchBar.Text);
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

		[Theory(DisplayName = "IsTextPredictionEnabled Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsTextPredictionEnabledInitializesCorrectly(bool isEnabled)
		{
			var searchBar = new SearchBarStub()
			{
				IsTextPredictionEnabled = isEnabled
			};

			await AttachAndRun(searchBar, async (searchBarHandler) =>
			{
				await AssertionExtensions.Wait(() => searchBarHandler.PlatformView.IsLoaded());
			});

			await ValidatePropertyInitValue(searchBar, () => searchBar.IsTextPredictionEnabled, GetNativeIsTextPredictionEnabled, isEnabled);
		}

		[Theory(DisplayName = "IsSpellCheckEnabled Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsSpellCheckEnabledInitializesCorrectly(bool isEnabled)
		{
			var searchBar = new SearchBarStub()
			{
				IsSpellCheckEnabled = isEnabled
			};

			await AttachAndRun(searchBar, async (searchBarHandler) =>
			{
				await AssertionExtensions.Wait(() => searchBarHandler.PlatformView.IsLoaded());
			});

			await ValidatePropertyInitValue(searchBar, () => searchBar.IsSpellCheckEnabled, GetNativeIsSpellCheckEnabled, isEnabled);
		}

		[Theory(DisplayName = "IsTextPredictionEnabled Updates Correctly")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsTextPredictionEnabledUpdatesCorrectly(bool setValue, bool unsetValue)
		{
			var searchBar = new SearchBarStub()
			{
				IsTextPredictionEnabled = setValue
			};

			await AttachAndRun(searchBar, async (searchBarHandler) =>
			{
				await AssertionExtensions.Wait(() => searchBarHandler.PlatformView.IsLoaded());
			});

			await ValidatePropertyUpdatesValue(
				searchBar,
				nameof(ISearchBar.IsTextPredictionEnabled),
				GetNativeIsTextPredictionEnabled,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "IsSpellCheckEnabled Updates Correctly")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsSpellCheckEnabledUpdatesCorrectly(bool setValue, bool unsetValue)
		{
			var searchBar = new SearchBarStub()
			{
				IsSpellCheckEnabled = setValue
			};

			await AttachAndRun(searchBar, async (searchBarHandler) =>
			{
				await AssertionExtensions.Wait(() => searchBarHandler.PlatformView.IsLoaded());
			});

			await ValidatePropertyUpdatesValue(
				searchBar,
				nameof(ISearchBar.IsSpellCheckEnabled),
				GetNativeIsSpellCheckEnabled,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "IsTextPredictionEnabled differs from IsSpellCheckEnabled")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task TextPredictionDiffersFromSpellChecking(bool textPredictionValue, bool spellCheckValue)
		{
			// Test to prevent: https://github.com/dotnet/maui/issues/8558
			var areValuesEqual = textPredictionValue == spellCheckValue;

			var searchBar = new SearchBarStub()
			{
				IsTextPredictionEnabled = textPredictionValue,
				IsSpellCheckEnabled = spellCheckValue
			};

			await AttachAndRun(searchBar, async (searchBarHandler) =>
			{
				await AssertionExtensions.Wait(() => searchBarHandler.PlatformView.IsLoaded());
			});

			var nativeTextPrediction = await GetValueAsync(searchBar, GetNativeIsTextPredictionEnabled);
			var nativeSpellChecking = await GetValueAsync(searchBar, GetNativeIsSpellCheckEnabled);

			Assert.Equal(areValuesEqual, (nativeTextPrediction == nativeSpellChecking));
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

		[Fact(DisplayName = "CancelButtonColor Initialize Correctly")]
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

			await AttachAndRun(searchBar, (handler) =>
			{
				var height = GetInputFieldHeight(handler);
				Assert.True(height >= 44);
			});
		}

		// TODO: JD - re-enable these tests for windows. Seems they were disabled because there is no implementation of "GetNativeKeyboard" in the .Windows test file
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
#if ANDROID || IOS || MACCATALYST
		// Android text and Chat keyboards are the same
		[InlineData(nameof(Keyboard.Chat), true)]
#else
		[InlineData(nameof(Keyboard.Chat), false)]
#endif
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
#elif IOS || MACCATALYST
		// On ios the text and default keyboards are the same
		[InlineData(nameof(Keyboard.Default), true)]
		[InlineData(nameof(Keyboard.Plain), false)]
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
#if IOS || MACCATALYST
		// On iOS the default and chat keyboard are the same
		[InlineData(nameof(Keyboard.Default), true)]
#else
		[InlineData(nameof(Keyboard.Default), false)]
#endif
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
#if ANDROID || IOS || MACCATALYST
		// Android & iOS text and Chat keyboards are the same
		[InlineData(nameof(Keyboard.Text), true)]
#else
		[InlineData(nameof(Keyboard.Text), false)]
#endif
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateChatKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var searchBar = new SearchBarStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(searchBar, () => expected, GetNativeIsChatKeyboard, expected);
		}
#endif

		[Category(TestCategory.SearchBar)]
		public class SearchBarTextStyleTests : TextStyleHandlerTests<SearchBarHandler, SearchBarStub>
		{
		}

		[Category(TestCategory.SearchBar)]
		public class SearchBarTextInputTests : TextInputHandlerTests<SearchBarHandler, SearchBarStub>
		{
			protected override void SetNativeText(SearchBarHandler searchBarHandler, string text) =>
				SearchBarHandlerTests.SetNativeText(searchBarHandler, text);

			protected override int GetCursorStartPosition(SearchBarHandler searchBarHandler) =>
				SearchBarHandlerTests.GetCursorStartPosition(searchBarHandler);

			protected override void UpdateCursorStartPosition(SearchBarHandler searchBarHandler, int position) =>
				SearchBarHandlerTests.UpdateCursorStartPosition(searchBarHandler, position);
		}

		// TODO: only iOS is working with the search bar focus tests
#if IOS || MACCATALYST
		[Category(TestCategory.SearchBar)]
		public class SearchBarFocusTests : FocusHandlerTests<SearchBarHandler, SearchBarStub, VerticalStackLayoutStub>
		{
			public SearchBarFocusTests()
			{
			}
		}
#endif

		class CustomSearchBarHandler : SearchBarHandler
		{
			// make a copy of the Core mappers because we don't want any Controls changes or to override us
			public static PropertyMapper<ISearchBar, ISearchBarHandler> TestMapper = new(Mapper);
			public static CommandMapper<ISearchBar, ISearchBarHandler> TestCommandMapper = new(CommandMapper);

			// make sure to use our mappers
			public CustomSearchBarHandler()
				: base(TestMapper, TestCommandMapper)
			{
			}
		}
	}
}