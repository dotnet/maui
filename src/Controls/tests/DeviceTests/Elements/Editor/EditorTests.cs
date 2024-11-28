using System;
using System.Linq;
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

			var layout = new VerticalStackLayout()
			{
				WidthRequest = 100,
				HeightRequest = 100,
				Children =
				{
					editor
				}
			};

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
			{
				var frame = editor.Frame;

				await Task.Yield();

				var initialHeight = editor.Height;

				editor.Text += Environment.NewLine + " Some new text" + Environment.NewLine + " Some new text" + Environment.NewLine;

				layout.WidthRequest = 1000;
				layout.HeightRequest = 1000;

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
			await ValidateEditorLayoutChangesForDisabledAutoSize(
				null,
				(control) => control.HeightRequest = 60,
				(control) =>
				{
					var frame = control.Frame;
					var desiredSize = control.DesiredSize;

					Assert.Equal(60, frame.Bottom, 0.5d);
					Assert.Equal(60, desiredSize.Height, 0.5d);
				});
		}

		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingWidth()
		{

			await ValidateEditorLayoutChangesForDisabledAutoSize(
				null,
				(control) => control.WidthRequest = 60,
				(control) =>
				{
					var frame = control.Frame;
					var desiredSize = control.DesiredSize;

					Assert.Equal(60, frame.Right, 0.5d);
					Assert.Equal(60, desiredSize.Width, 0.5d);
				});
		}

		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingMargin()
		{
			await ValidateEditorLayoutChangesForDisabledAutoSize(
				null,
				(control) => control.Margin = new Thickness(5, 5, 5, 5),
				(control) =>
				{
					var frame = control.Frame;
					var desiredSize = control.DesiredSize;

					Assert.Equal(55, frame.Right, 0.5d);
					Assert.Equal(60, desiredSize.Width, 0.5d);
				});
		}

		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingMinHeight()
		{
			await ValidateEditorLayoutChangesForDisabledAutoSize(
				(control) =>
				{
					control.HeightRequest = Primitives.Dimension.Unset;
				},
				(control) => control.MinimumHeightRequest = 100,
				(control) =>
				{
					var frame = control.Frame;
					var desiredSize = control.DesiredSize;

					Assert.Equal(100, frame.Bottom, 0.5d);
					Assert.Equal(100, desiredSize.Height, 0.5d);
				});
		}

		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingMaxHeight()
		{
			await ValidateEditorLayoutChangesForDisabledAutoSize(
				(control) =>
				{
					control.HeightRequest = Primitives.Dimension.Unset;
				},
				(control) => control.MaximumHeightRequest = 10,
				(control) =>
				{
					var frame = control.Frame;
					var desiredSize = control.DesiredSize;

					Assert.Equal(10, frame.Bottom, 0.5d);
					Assert.Equal(10, desiredSize.Height, 0.5d);
				});
		}

		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingMinWidth()
		{
			await ValidateEditorLayoutChangesForDisabledAutoSize(
				(control) =>
				{
					control.WidthRequest = Primitives.Dimension.Unset;
				},
				(control) => control.MinimumWidthRequest = 100,
				(control) =>
				{
					var frame = control.Frame;
					var desiredSize = control.DesiredSize;

					Assert.Equal(100, frame.Width, 0.5d);
					Assert.Equal(100, desiredSize.Width, 0.5d);
				});
		}

		[Fact]
		public async Task EditorMeasureUpdatesWhenChangingMaxWidth()
		{
			await ValidateEditorLayoutChangesForDisabledAutoSize(
				(control) =>
				{
					control.WidthRequest = Primitives.Dimension.Unset;
					control.Text = String.Join(",", Enumerable.Range(0, 100).Select(x => "a").ToArray());
				},
				(control) =>
				{
					control.MaximumWidthRequest = 10;
				},
				(control) =>
				{
					var frame = control.Frame;
					var desiredSize = control.DesiredSize;

					Assert.Equal(10, frame.Width, 0.5d);
					Assert.Equal(10, desiredSize.Width, 0.5d);
				});
		}

		async Task ValidateEditorLayoutChangesForDisabledAutoSize(
			Action<Editor> arrange,
			Action<Editor> act,
			Action<Editor> assert
			)
		{
			SetupBuilder();
			var control = new Editor()
			{
				AutoSize = EditorAutoSizeOption.Disabled,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};

			control.HeightRequest = 50;
			control.WidthRequest = 50;
			control.MinimumWidthRequest = 0;
			control.MaximumWidthRequest = 100;
			control.MinimumHeightRequest = 0;
			control.MaximumHeightRequest = 100;

			IView layout = new VerticalStackLayout()
			{
				HeightRequest = 100,
				WidthRequest = 100,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
				Children =
				{
					control
				}
			};

			arrange?.Invoke(control);

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
			{
				await Task.Yield();
				var frame = control.Frame;
				act.Invoke(control);
				await WaitForUIUpdate(frame, control);
				assert.Invoke(control);
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
