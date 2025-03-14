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

		Task<float> GetPlatformOpacity(EditorHandler editorHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(editorHandler);
				return nativeView.Alpha;
			});
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

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a Editor should match with native Translation")]
		public async Task EditorTranslationConsistent()
		{
			var editor = new Editor()
			{
				Text = "Editor Test",
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var translation = nativeView.TranslationX;
				var density = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Density;
				var expectedInPixels = density * editor.TranslationX;

				Assert.Equal(expectedInPixels, translation, 1.0);

				var translationY = nativeView.TranslationY;
				var expectedYInPixels = density * editor.TranslationY;
				Assert.Equal(expectedYInPixels, translationY, 1.0);
			});
		}
	}
}
