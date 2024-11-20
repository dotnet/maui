using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		static AppCompatEditText GetPlatformControl(EntryHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		static void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).SetTextKeepState(text);

		static int GetPlatformCursorPosition(EntryHandler entryHandler)
		{
			var editText = GetPlatformControl(entryHandler);

			if (editText != null)
				return editText.SelectionEnd;

			return -1;
		}

		static int GetPlatformSelectionLength(EntryHandler entryHandler)
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
		[Category(TestCategory.Entry)]
		public async Task UpdateTextWithTextLongerThanMaxLength()
		{
			string longText = "A text longer than 4 characters";
			var entry = new Entry
			{
				MaxLength = 4,
			};

			await SetValueAsync<string, EntryHandler>(entry, longText, SetPlatformText);

			Assert.Equal(longText[..4], entry.Text);
		}
	}
}
