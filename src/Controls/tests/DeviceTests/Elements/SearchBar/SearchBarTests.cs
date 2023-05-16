using System;
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

#if false
		// TODO: The search bar controls are composite controls and need to be attached to the UI to run
		[Category(TestCategory.SearchBar)]
		[Category(TestCategory.TextInput)]
		public class SearchBarTextInputTests : TextInputTests<SearchBarHandler, SearchBar>
		{
			protected override int GetPlatformSelectionLength(SearchBarHandler handler) =>
				SearchBarTests.GetPlatformSelectionLength(handler);

			protected override int GetPlatformCursorPosition(SearchBarHandler handler) =>
				SearchBarTests.GetPlatformCursorPosition(handler);
		}
#endif

		[Category(TestCategory.SearchBar)]
		[Category(TestCategory.TextInput)]
		public class SearchBarTextInputTextTransformTests : TextInputTextTransformTests<SearchBarHandler, SearchBar>
		{
			protected override int GetPlatformSelectionLength(SearchBarHandler handler) =>
				SearchBarTests.GetPlatformSelectionLength(handler);

			protected override int GetPlatformCursorPosition(SearchBarHandler handler) =>
				SearchBarTests.GetPlatformCursorPosition(handler);

			protected override Task<string> GetPlatformText(SearchBarHandler handler) =>
				SearchBarTests.GetPlatformText(handler);
		}

		[Category(TestCategory.SearchBar)]
		[Category(TestCategory.TextInput)]
		[Collection(RunInNewWindowCollection)]
		public class SearchBarTextInputFocusTests : TextInputFocusTests<SearchBarHandler, SearchBar>
		{
		}
	}
}
