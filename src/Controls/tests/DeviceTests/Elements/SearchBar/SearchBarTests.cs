using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SearchBar)]
	public partial class SearchBarTests : ControlsHandlerTestBase
	{
		// TODO: remove these 2 tests and use SearchBarTextInputTests below

		[Theory(DisplayName = "Text is Transformed Correctly at Initialization")]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new SearchBar() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<SearchBarHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory(DisplayName = "Text is Transformed Correctly after Initialization")]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new SearchBar() { Text = text };
			var handler = await CreateHandlerAsync<SearchBarHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

#if MACCATALYST || IOS
		// Only Mac Catalyst and iOS needs the CancelButtonColor nuanced handling verifying
		[Fact(DisplayName = "CancelButtonColor is set correctly")]
		public async Task CancelButtonColorSetCorrectly()
		{
			var expected = Graphics.Colors.Red;

			var searchBar = new SearchBar()
			{
				CancelButtonColor = expected,
				Text = "CancelButtonColor is set correctly"
			};

			var actualColor = await GetPlatformCancelButtonColor(await CreateHandlerAsync<SearchBarHandler>(searchBar));

			Assert.Equal(expected, actualColor);
		}
#endif

#if WINDOWS
		// Only Windows needs the IsReadOnly workaround for MaxLength==0 to prevent text from being entered
		[Fact]
		public async Task MaxLengthIsReadOnlyValueTest()
		{
			SearchBar searchBar = new SearchBar();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<SearchBarHandler>(searchBar);
				var platformControl = GetPlatformControl(handler);

				searchBar.MaxLength = 0;
				Assert.True(MauiAutoSuggestBox.GetIsReadOnly(platformControl));
				searchBar.IsReadOnly = false;
				Assert.True(MauiAutoSuggestBox.GetIsReadOnly(platformControl));

				searchBar.MaxLength = 10;
				Assert.False(MauiAutoSuggestBox.GetIsReadOnly(platformControl));
				searchBar.IsReadOnly = true;
				Assert.True(MauiAutoSuggestBox.GetIsReadOnly(platformControl));
			});
		}
#endif

		[Fact]
		[Description("The Opacity property of a SearchBar should match with native Opacity")]
		public async Task VerifySearchBarOpacityProperty()
		{
			var searchBar = new SearchBar
			{
				Opacity = 0.35f
			};
			var expectedValue = searchBar.Opacity;

			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

		[Fact]
		[Description("The IsVisible property of a SearchBar should match with native IsVisible")]
		public async Task VerifySearchBarIsVisibleProperty()
		{
			var searchBar = new SearchBar
			{
				IsVisible = false
			};
			var expectedValue = searchBar.IsVisible;

			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			await InvokeOnMainThreadAsync(async () =>
   			{
				   var isVisible = await GetPlatformIsVisible(handler);
				   Assert.Equal(expectedValue, isVisible);
			   });
		}

#if false
		// TODO: The search bar controls are composite controls and need to be attached to the UI to run
		[Category(TestCategory.SearchBar)]
		[Category(TestCategory.TextInput)]
		[Collection(RunInNewWindowCollection)]
		public class SearchBarTextInputTests : TextInputTests<SearchBarHandler, SearchBar>
		{
			protected override int GetPlatformSelectionLength(SearchBarHandler handler) =>
				SearchBarTests.GetPlatformSelectionLength(handler);

			protected override int GetPlatformCursorPosition(SearchBarHandler handler) =>
				SearchBarTests.GetPlatformCursorPosition(handler);

			protected override Task<string> GetPlatformText(SearchBarHandler handler) =>
				SearchBarTests.GetPlatformText(handler);
		}
#endif
	}
}
