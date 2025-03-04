using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;
using System.ComponentModel;

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

		[Fact]
		[Description("The IsEnabled property of a Editor should match with native IsEnabled")]		
		public async Task VerifyEditorIsEnabledProperty()
		{
			var editor = new Editor
			{
				IsEnabled = false
			};
			var expectedValue = editor.IsEnabled;

			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.Enabled;

				Assert.Equal(expectedValue, isEnabled);
			});		
		}		
	}
}
