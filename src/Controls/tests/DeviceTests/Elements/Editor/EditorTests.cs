using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Editor)]
	public partial class EditorTests : ControlsHandlerTestBase
	{
#if !IOS && !MACCATALYST
		// iOS is broken until this point
		// https://github.com/dotnet/maui/issues/3425
		[Theory]
		[InlineData(EditorAutoSizeOption.Disabled)]
		[InlineData(EditorAutoSizeOption.TextChanges)]
		public async Task AutoSizeInitializesCorrectly(EditorAutoSizeOption option)
		{
			var editor = new Editor
			{
				AutoSize = option,
				Text = "Test"
			};

			IView layout = new VerticalStackLayout()
			{
				Children =
				{
					editor
				}
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, (_) =>
			{
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(1000, 1000)));
				var initialHeight = editor.Height;

				editor.Text += Environment.NewLine + " Some new text" + Environment.NewLine;
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(1000, 1000)));

				if (option == EditorAutoSizeOption.Disabled)
					Assert.Equal(initialHeight, editor.Height);
				else
					Assert.True(initialHeight < editor.Height);

				return Task.CompletedTask;
			});
		}
#endif

#if WINDOWS
		// Only Windows needs the IsReadOnly workaround for MaxLength==0 to prevent text from being entered
		[Fact]
		public async Task MaxLengthIsReadOnlyValueTest()
		{
			Editor editor = new Editor();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<EditorHandler>(editor);
				var platformControl = GetPlatformControl(handler);

				editor.MaxLength = 0;
				Assert.True(platformControl.IsReadOnly);
				editor.IsReadOnly = false;
				Assert.True(platformControl.IsReadOnly);

				editor.MaxLength = 10;
				Assert.False(platformControl.IsReadOnly);
				editor.IsReadOnly = true;
				Assert.True(platformControl.IsReadOnly);
			});
		}
#endif

		[Category(TestCategory.Editor)]
		[Category(TestCategory.TextInput)]
		public class EditorTextInputTests : TextInputTests<EditorHandler, Editor>
		{
			protected override int GetPlatformSelectionLength(EditorHandler handler) =>
				EditorTests.GetPlatformSelectionLength(handler);

			protected override int GetPlatformCursorPosition(EditorHandler handler) =>
				EditorTests.GetPlatformCursorPosition(handler);
		}

		[Category(TestCategory.Editor)]
		[Category(TestCategory.TextInput)]
		public class EditorTextInputTextTransformTests : TextInputTextTransformTests<EditorHandler, Editor>
		{
			protected override int GetPlatformSelectionLength(EditorHandler handler) =>
				EditorTests.GetPlatformSelectionLength(handler);

			protected override int GetPlatformCursorPosition(EditorHandler handler) =>
				EditorTests.GetPlatformCursorPosition(handler);

			protected override Task<string> GetPlatformText(EditorHandler handler) =>
				EditorTests.GetPlatformText(handler);
		}

		[Collection(RunInNewWindowCollection)]
		[Category(TestCategory.Editor)]
		[Category(TestCategory.TextInput)]
		public class EditorTextInputFocusTests : TextInputFocusTests<EditorHandler, Editor>
		{
		}
	}
}
