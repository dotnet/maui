using System.Threading.Tasks;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
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

		// This test will only run if the Android Manifest of the Controls.DeviceTests project is edited to have android:supportsRtl="false"
		[Fact(DisplayName = "Horizontal text aligned when RTL is not supported")]
		public async Task HorizontalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var entry = new Entry { Text = "Foo", HorizontalTextAlignment = TextAlignment.Center };

			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var platformEntry = GetPlatformControl(handler);

			Assert.False(platformEntry.Gravity.HasFlag(GravityFlags.Start), "Entry should not have the Start flag.");
			Assert.False(platformEntry.Gravity.HasFlag(GravityFlags.End), "Entry should not have the End flag.");
			Assert.True(platformEntry.Gravity.HasFlag(GravityFlags.CenterHorizontal), "Entry should have the CenterHorizontal flag.");
		}

		// This test will only run if the Android Manifest of the Controls.DeviceTests project is edited to have android:supportsRtl="false"
		[Fact(DisplayName = "Vertical text aligned when RTL is not supported")]
		public async Task VerticalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var entry = new Entry { Text = "Foo", VerticalTextAlignment = TextAlignment.Center };

			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var platformEntry = GetPlatformControl(handler);

			Assert.False(platformEntry.Gravity.HasFlag(GravityFlags.Top), "Entry should not have the Top flag.");
			Assert.False(platformEntry.Gravity.HasFlag(GravityFlags.Bottom), "Entry should not have the Bottom flag.");
			Assert.True(platformEntry.Gravity.HasFlag(GravityFlags.CenterVertical), "Entry should only have the CenterVertical flag.");
		}
	}
}
