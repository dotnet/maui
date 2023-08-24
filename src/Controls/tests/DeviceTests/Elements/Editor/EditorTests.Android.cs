using System.Threading.Tasks;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class EditorTests
	{
		static AppCompatEditText GetPlatformControl(EditorHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(EditorHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		static void SetPlatformText(EditorHandler editorHandler, string text) =>
			GetPlatformControl(editorHandler).SetTextKeepState(text);

		static int GetPlatformCursorPosition(EditorHandler editorHandler)
		{
			var textView = GetPlatformControl(editorHandler);

			if (textView != null)
				return textView.SelectionStart;

			return -1;
		}

		static int GetPlatformSelectionLength(EditorHandler editorHandler)
		{
			var textView = GetPlatformControl(editorHandler);

			if (textView != null)
				return textView.SelectionEnd - textView.SelectionStart;

			return -1;
		}

		[Fact]
		public async Task CursorPositionPreservedWhenTextTransformPresent()
		{
			var editor = new Editor
			{
				Text = "TET",
				TextTransform = TextTransform.Uppercase
			};

			await SetValueAsync<int, EditorHandler>(editor, 2, (h, s) => h.PlatformView.SetSelection(2));

			Assert.Equal(2, editor.CursorPosition);

			await SetValueAsync<string, EditorHandler>(editor, "TEsT", SetPlatformText);

			Assert.Equal(2, editor.CursorPosition);
		}

		// This test will only run if the Android Manifest of the Controls.DeviceTests project is edited to have android:supportsRtl="false"
		[Fact(DisplayName = "Horizontal text aligned when RTL is not supported")]
		public async Task HorizontalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var editor = new Editor { Text = "Foo", HorizontalTextAlignment = TextAlignment.Center };

			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var platformEditor = GetPlatformControl(handler);

			Assert.False(platformEditor.Gravity.HasFlag(GravityFlags.Start), "Editor should not have the Start flag.");
			Assert.False(platformEditor.Gravity.HasFlag(GravityFlags.End), "Editor should not have the End flag.");
			Assert.True(platformEditor.Gravity.HasFlag(GravityFlags.CenterHorizontal), "Editor should have the CenterHorizontal flag.");
		}

		//// This test will only run if the Android Manifest of the Controls.DeviceTests project is edited to have android:supportsRtl="false"
		[Fact(DisplayName = "Vertical text aligned when RTL is not supported")]
		public async Task VerticalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var editor = new Editor { Text = "Foo", VerticalTextAlignment = TextAlignment.Center };

			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var platformEditor = GetPlatformControl(handler);

			Assert.False(platformEditor.Gravity.HasFlag(GravityFlags.Top), "Editor should not have the Top flag.");
			Assert.False(platformEditor.Gravity.HasFlag(GravityFlags.Bottom), "Editor should not have the Bottom flag.");
			Assert.True(platformEditor.Gravity.HasFlag(GravityFlags.CenterVertical), "Editor should only have the CenterVertical flag.");
		}
	}
}
