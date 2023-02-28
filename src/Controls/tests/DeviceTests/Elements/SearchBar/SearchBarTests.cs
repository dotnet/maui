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
		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new SearchBar() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<SearchBarHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new SearchBar() { Text = text };
			var handler = await CreateHandlerAsync<SearchBarHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

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
	}
}
