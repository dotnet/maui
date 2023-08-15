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
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Editor, EditorHandler>();
				});
			});
		}

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

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (_) =>
			{
				var frame = editor.Frame;

				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(100, 100)));
				await WaitForUIUpdate(frame, editor);

				var initialHeight = editor.Height;

				editor.Text += Environment.NewLine + " Some new text" + Environment.NewLine + " Some new text" + Environment.NewLine;

				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(1000, 1000)));
				await WaitForUIUpdate(frame, editor);

				if (option == EditorAutoSizeOption.TextChanges)
					Assert.True(initialHeight < editor.Height);
				else
					Assert.Equal(initialHeight, editor.Height);
			});
		}

		static async Task WaitForUIUpdate(Graphics.Rect frame, Editor collectionView, int timeout = 1000, int interval = 100)
		{
			// Wait for layout to happen
			while (collectionView.Frame == frame && timeout >= 0)
			{
				await Task.Delay(interval);
				timeout -= interval;
			}
		}

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
		[Collection(RunInNewWindowCollection)]
		public class EditorTextInputTests : TextInputTests<EditorHandler, Editor>
		{
			protected override int GetPlatformSelectionLength(EditorHandler handler) =>
				EditorTests.GetPlatformSelectionLength(handler);

			protected override int GetPlatformCursorPosition(EditorHandler handler) =>
				EditorTests.GetPlatformCursorPosition(handler);

			protected override Task<string> GetPlatformText(EditorHandler handler) =>
				EditorTests.GetPlatformText(handler);
		}

		[Category(TestCategory.Editor)]
		[Category(TestCategory.TextInput)]
		[Collection(RunInNewWindowCollection)]
		public class EditorTextInputFocusTests : TextInputFocusTests<EditorHandler, Editor>
		{
		}
	}
}
