using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		AppCompatEditText GetPlatformControl(EntryHandler handler) =>
			handler.PlatformView;

		Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).SetTextKeepState(text);

		int GetPlatformCursorPosition(EntryHandler entryHandler)
		{
			var editText = GetPlatformControl(entryHandler);

			if (editText != null)
				return editText.SelectionEnd;

			return -1;
		}

		int GetPlatformSelectionLength(EntryHandler entryHandler)
		{
			var editText = GetPlatformControl(entryHandler);

			if (editText != null)
				return editText.SelectionEnd - editText.SelectionStart;

			return -1;
		}

		[Fact]
		public async Task CursorPositionPreservedWhenTextTransformPresent()
		{
			var entry = new Entry
			{
				Text = "TET",
				TextTransform = TextTransform.Uppercase
			};

			await SetValueAsync<int, EntryHandler>(entry, 2, (h, s) => h.PlatformView.SetSelection(2));

			Assert.Equal(2, entry.CursorPosition);

			await SetValueAsync<string, EntryHandler>(entry, "TEsT", SetPlatformText);

			Assert.Equal(2, entry.CursorPosition);
		}

		[Fact]
		public async Task ShowsKeyboardOnFocus()
		{
			var entry = new Entry();

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<EntryHandler>(entry);

				await handler.PlatformView.AttachAndRun(async () =>
				{
					entry.Focus();
					await AssertionExtensions.WaitForKeyboardToShow(handler.PlatformView);

					// Test that keyboard reappears when refocusing on an already focused TextInput control
					await AssertionExtensions.HideKeyboardForView(handler.PlatformView);
					await AssertionExtensions.WaitForKeyboardToHide(handler.PlatformView);
					entry.Focus();
					await AssertionExtensions.WaitForKeyboardToShow(handler.PlatformView);
				});
			});
		}
	}
}
