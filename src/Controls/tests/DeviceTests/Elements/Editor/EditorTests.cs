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

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
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
		
		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingHeight()
		{
			SetupBuilder();
			var control = new Editor();
			control.HeightRequest = 50;
			control.WidthRequest = 50;

			IView layout = new VerticalStackLayout()
			{
				Children =
				{
					control
				}
			};

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
			{
				var frame = control.Frame;

				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(100, 100)));
				await Task.Yield();
				control.HeightRequest = 60;
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(100, 100)));
				await Task.Yield();

				var frame2 = control.Frame;
				var desiredSize = control.DesiredSize;
				var height = control.Height;
				var width = control.Width;

				Assert.Equal(60, frame2.Bottom, 0.5d);
				Assert.Equal(60, desiredSize.Height, 0.5d);
			});
		}
		
		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingWidth()
		{
			SetupBuilder();
			var control = new Editor();
			control.HeightRequest = 50;
			control.WidthRequest = 50;

			IView layout = new VerticalStackLayout()
			{
				Children =
				{
					control
				}
			};

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
			{
				var frame = control.Frame;

				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(100, 100)));
				await Task.Yield();
				control.WidthRequest = 60;
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(100, 100)));
				await Task.Yield();

				var frame2 = control.Frame;
				var desiredSize = control.DesiredSize;
				var height = control.Height;
				var width = control.Width;

				Assert.Equal(60, frame2.Right, 0.5d);
				Assert.Equal(60, desiredSize.Width, 0.5d);
			});
		}

		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingMargin()
		{
			SetupBuilder();
			var control = new Editor();
			control.HeightRequest = 50;
			control.WidthRequest = 50;

			IView layout = new VerticalStackLayout()
			{
				Children =
				{
					control
				}
			};

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
			{
				var frame = control.Frame;

				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(100, 100)));
				await Task.Yield();
				control.Margin = new Thickness(5,5,5,5);
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(100, 100)));
				await Task.Yield();

				var frame2 = control.Frame;
				var desiredSize = control.DesiredSize;
				var height = control.Height;
				var width = control.Width;

				Assert.Equal(55, frame2.Right, 0.5d);
				Assert.Equal(60, desiredSize.Width, 0.5d);
			});
		}

		static async Task WaitForUIUpdate(Graphics.Rect frame, Editor editor, int timeout = 1000, int interval = 100)
		{
			// Wait for layout to happen
			while (editor.Frame == frame && timeout >= 0)
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
	}
}
