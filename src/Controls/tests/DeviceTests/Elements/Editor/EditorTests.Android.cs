using System.ComponentModel;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
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

		Task<float> GetPlatformOpacity(EditorHandler editorHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(editorHandler);
				return nativeView.Alpha;
			});
		}

		Task<bool> GetPlatformIsVisible(EditorHandler editorHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(editorHandler);
				return nativeView.Visibility == Android.Views.ViewStates.Visible;
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

		[Fact]
		[Description("The ScaleX property of a Editor should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var editor = new Editor() { ScaleX = 0.45f };
			var expected = editor.ScaleX;
			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var PlatformEditor = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => PlatformEditor.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a Editor should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var editor = new Editor() { ScaleY = 1.23f };
			var expected = editor.ScaleY;
			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var PlatformEditor = GetPlatformControl(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => PlatformEditor.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a Editor should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var editor = new Editor() { Scale = 2.0f };
			var expected = editor.Scale;
			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var PlatformEditor = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => PlatformEditor.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => PlatformEditor.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a Editor should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var editor = new Editor() { RotationX = 33.0 };
			var expected = editor.RotationX;
			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var PlatformEditor = GetPlatformControl(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => PlatformEditor.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a Editor should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var editor = new Editor() { RotationY = 87.0 };
			var expected = editor.RotationY;
			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var PlatformEditor = GetPlatformControl(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => PlatformEditor.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a Editor should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var editor = new Editor() { Rotation = 23.0 };
			var expected = editor.Rotation;
			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var PlatformEditor = GetPlatformControl(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => PlatformEditor.Rotation);
			Assert.Equal(expected, platformRotation);
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
				AssertTranslationMatches(nativeView, editor.TranslationX, editor.TranslationY);
			});
		}
	}
}
