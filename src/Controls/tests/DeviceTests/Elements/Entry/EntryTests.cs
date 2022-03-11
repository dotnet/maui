using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Entry)]
	public partial class EntryTests : HandlerTestBase
	{
		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Entry() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<EntryHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new Entry() { Text = text };
			var handler = await CreateHandlerAsync<EntryHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

		[Fact]
		public async Task CursorPositionDoesntResetWhenNativeTextValueChanges()
		{
			var textInput = new Entry()
			{
				Text = "Hello"
			};


			int cursorPosition = 0;
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<EntryHandler>(textInput);
				UpdateCursorStartPosition(handler, 5);
				handler.UpdateValue(nameof(ITextInput.Text));
				cursorPosition = GetCursorStartPosition(handler);
			});

			Assert.Equal(5, cursorPosition);
		}

		[Fact]
		public async Task CursorPositionResetsToZeroAfterChangingText()
		{
			var textInput = new Entry()
			{
				Text = "Hello"
			};


			int cursorPosition = 0;
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<EntryHandler>(textInput);
				UpdateCursorStartPosition(handler, 5);
				textInput.Text = "hel";
				cursorPosition = GetCursorStartPosition(handler);
			});

			// iOS won't reset your cursor position when changing the value.
			// We could force iOS to act like winui/android
			// but that starts to become a rabbit hole.
			// If the developer cares they can use the cursor APIs
			// to specifically move the cursor where they want it to be
#if IOS
			Assert.Equal(3, cursorPosition);
#else
			Assert.Equal(0, cursorPosition);
#endif
		}
	}
}
