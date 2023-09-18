using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Frame)]
	public partial class FrameTests : ControlsHandlerTestBase
	{
		// This a hard-coded legacy value
		const int FrameBorderThickness = 1;

		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<StackLayout, LayoutHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Frame, FrameRenderer>();
					handlers.AddHandler<Frame, FrameRenderer>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		[Fact(DisplayName = "Basic Frame Test")]
		public async Task BasicFrameTest()
		{
			SetupBuilder();

			var frame = new Frame()
			{
				HeightRequest = 300,
				WidthRequest = 300,
				Content = new Label()
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Text = "Hello Frame"
				}
			};

			var labelFrame =
					await AttachAndRun<Rect>(frame, async (handler) =>
					{
						(frame as IView).Measure(300, 300);
						(frame as IView).Arrange(new Graphics.Rect(0, 0, 300, 300));

						await OnFrameSetToNotEmpty(frame.Content);

						return frame.Content.Frame;
					});


			// validate label is centered in the frame
			Assert.True(Math.Abs(((300 - labelFrame.Width) / 2) - labelFrame.X) < 1);
			Assert.True(Math.Abs(((300 - labelFrame.Height) / 2) - labelFrame.Y) < 1);
		}

		[Fact(DisplayName = "Frame With Entry Measures")]
		public async Task FrameWithEntryMeasuresCorrectly()
		{
			SetupBuilder();

			var entry = new Entry()
			{
				Text = "Hello Frame"
			};

			var frame = new Frame()
			{
				Content = entry
			};

			var layout = new StackLayout()
			{
				Children =
				{
					frame
				}
			};

			var layoutFrame = await LayoutFrame(layout, frame, double.PositiveInfinity, double.PositiveInfinity);

			Assert.True(entry.Width > 0);
			Assert.True(entry.Height > 0);

			Assert.True(frame.Width > 0);
			Assert.True(frame.Height > 0);

			Assert.True(layout.Width > 0);
			Assert.True(layout.Height > 0);
		}

		[Theory(DisplayName = "Frame BackgroundColor Initializes Correctly")]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		[InlineData("#000000")]
		public async Task FrameBackgroundColorInitializesCorrectly(string colorHex)
		{
			SetupBuilder();

			var expectedColor = Graphics.Color.FromArgb(colorHex);

			var frame = new Frame()
			{
				BackgroundColor = expectedColor,
				HeightRequest = 300,
				WidthRequest = 300,
				Content = new Label()
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Text = "BackgroundColor"
				}
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = frame.ToPlatform(MauiContext);
				platformView.AssertContainsColor(expectedColor, MauiContext);
			});
		}

		[Theory(DisplayName = "Frame BorderColor Initializes Correctly")]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		[InlineData("#000000")]
		public async Task FrameBorderColorInitializesCorrectly(string colorHex)
		{
			SetupBuilder();

			var expectedColor = Graphics.Color.FromArgb(colorHex);

			var frame = new Frame()
			{
				BorderColor = expectedColor,
				HeightRequest = 300,
				WidthRequest = 300,
				Content = new Label()
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Text = "BorderColor"
				}
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = frame.ToPlatform(MauiContext);
				platformView.AssertContainsColor(expectedColor, MauiContext);
			});
		}

		[Fact(DisplayName = "Frame Respects minimum height/width")]
		public async Task FrameRespectsMinimums()
		{
			SetupBuilder();

			var content = new Button { Text = "Hey", WidthRequest = 50, HeightRequest = 50 };

			var frame = new Frame()
			{
				Content = content,
				MinimumHeightRequest = 100,
				MinimumWidthRequest = 100,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start
			};

			var layout = new StackLayout()
			{
				Children =
				{
					frame
				}
			};

			var layoutFrame = await LayoutFrame(layout, frame, 500, 500);

			Assert.True(100 <= layoutFrame.Height);
			Assert.True(100 <= layoutFrame.Width);
		}

		[Fact]
		public async Task FrameDoesNotInterpretConstraintsAsMinimums()
		{
			SetupBuilder();

			var content = new Button { Text = "Hey", WidthRequest = 50, HeightRequest = 50 };

			var frame = new Frame()
			{
				Content = content,
				MinimumHeightRequest = 100,
				MinimumWidthRequest = 100,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start
			};

			var layout = new StackLayout()
			{
				Children =
				{
					frame
				}
			};

			var layoutFrame = await LayoutFrame(layout, frame, 500, 500);

			Assert.True(500 > layoutFrame.Width);
			Assert.True(500 > layoutFrame.Height);
		}

		[Fact]
		public async Task FrameIncludesBorderThickness()
		{
			SetupBuilder();

			double contentSize = 50;
			var content = new Label { Text = "Hey", WidthRequest = contentSize, HeightRequest = contentSize };

			var frame = new Frame()
			{
				Content = content,
				BorderColor = Colors.Black,
				CornerRadius = 10,
				Padding = new Thickness(0),
				Margin = new Thickness(0),
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start
			};

			var layout = new StackLayout()
			{
				Children =
				{
					frame
				}
			};

			var layoutFrame = await LayoutFrame(layout, frame, 500, 500);

			// Because this Frame has a border color, we expect the border to show up. So we need to account for its size
			var expected = contentSize + (FrameBorderThickness * 2);

			// We're checking the Frame size within a tolerance of 1; between screen density of test devices and rounding issues,
			// it's not going to be exactly `expected`, but it should be close.
			Assert.Equal(expected, layoutFrame.Width, 1.0d);
			Assert.Equal(expected, layoutFrame.Height, 1.0d);
		}

		[Fact]
		public async Task FrameIncludesPadding()
		{
			SetupBuilder();

			double contentSize = 50;
			var content = new Label { Text = "Hey", WidthRequest = contentSize, HeightRequest = contentSize };
			var padding = 10;

			var frame = new Frame()
			{
				Content = content,
				BorderColor = Colors.Black,
				CornerRadius = 10,
				Padding = new Thickness(padding),
				Margin = new Thickness(0),
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start
			};

			var layout = new StackLayout()
			{
				Children =
				{
					frame
				}
			};

			var layoutFrame = await LayoutFrame(layout, frame, 500, 500);

			// The expected content size should add padding to every direction
			var expected = contentSize + (padding * 2) + (FrameBorderThickness * 2);

			// We're checking the Frame size within a tolerance of 1; between screen density of test devices and rounding issues,
			// it's not going to be exactly `expected`, but it should be close.
			Assert.Equal(expected, layoutFrame.Width, 1.0d);
			Assert.Equal(expected, layoutFrame.Height, 1.0d);
		}

#if !ANDROID && !IOS
		[Fact]
		public async Task FrameResizesItsContents()
		{
			SetupBuilder();

			var originalLayoutDimensions = 300;
			var shrunkWindowWidth = originalLayoutDimensions - 100;

			var content = new Label
			{
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
			};

			var frame = new Frame()
			{
				Content = content,
				BorderColor = Colors.Black,
				CornerRadius = 10,
				Padding = new Thickness(0),
				Margin = new Thickness(0),
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Start
			};

			var layout = new StackLayout()
			{
				Children = { frame }
			};

			await CreateHandlerAndAddToWindow<IPlatformViewHandler>(layout, async (handler) =>
			{
				// Place the frame in a spacious container in a large window
				var size = (layout as IView).Measure(originalLayoutDimensions, originalLayoutDimensions);
				var rect = new Graphics.Rect(0, 0, size.Width, size.Height);
				(layout as IView).Arrange(rect);
				await OnFrameSetToNotEmpty(layout);
				await OnFrameSetToNotEmpty(frame);

				// Measure frame when it was first rendered in a spacious container
				var frameControlSize = (frame.Handler as IPlatformViewHandler).PlatformView.GetBoundingBox();
				var originalFrameHeight = frameControlSize.Height;
				Assert.True(frameControlSize.Width > 0);
				// Resize window to be smaller, forcing the frame to shrink (and wait for the changes to reflect)
				layout.Window.Width = shrunkWindowWidth;
				await Task.Delay(2000);

				// Ensure frame is within the window dimensions
				frameControlSize = (frame.Handler as IPlatformViewHandler).PlatformView.GetBoundingBox();
				Assert.True((frameControlSize.Width > 0) && (frameControlSize.Width < shrunkWindowWidth));

				// If the frame's height changed (it wrapped some text), ensure it hasn't shrunk
				Assert.True(frameControlSize.Height >= originalFrameHeight);
			});
		}
#endif

		[Theory]
		[InlineData(500, 500)]
		[InlineData(500, double.PositiveInfinity)]
		[InlineData(double.PositiveInfinity, double.PositiveInfinity)]
		[InlineData(double.PositiveInfinity, 500)]
		public async Task FramesWithinFrames(double widthConstraint, double heightConstraint)
		{
			SetupBuilder();

			var frameMargin = 30;
			var middleFrameMarginWidth = 0;
			var middleFrameMarginHeight = 0;
			var outerFrameMargin = 20;

			var content = new Label { Text = "Hey", FontSize = 12 };

			var frame = new Frame()
			{
				Content = content,
				BackgroundColor = Colors.White,
				CornerRadius = 0,
				HasShadow = false,
				Padding = new Thickness(0),
				Margin = new Thickness(frameMargin)
			};

			var middleFrame = new Frame()
			{
				Content = frame,
				BackgroundColor = Colors.Blue,
				CornerRadius = 20,
				Padding = new Thickness(0),
				Margin = new Thickness(middleFrameMarginWidth, middleFrameMarginHeight),
				HasShadow = false
			};

			var outerFrame = new Frame()
			{
				Content = middleFrame,
				BorderColor = Colors.Black,
				CornerRadius = 0,
				Padding = new Thickness(0),
				Margin = new Thickness(outerFrameMargin),
				HasShadow = false
			};

			var layout = new StackLayout()
			{
				Children =
				{
					outerFrame
				}
			};

			var layoutFrame = await LayoutFrame(layout, outerFrame, widthConstraint, heightConstraint);

			// frameBorderWidth is included once because only the outer frame has a border color set, so it's the only one which
			// will have a border.
			var minExpectedWidth = (2 * frameMargin) + (2 * middleFrameMarginWidth) + (2 * outerFrameMargin) + (2 * FrameBorderThickness);
			var minExpectedHeight = (2 * frameMargin) + (2 * middleFrameMarginHeight) + (2 * outerFrameMargin) + (2 * FrameBorderThickness);

			// This test is specifically to guard against Android measurement situations where the nested frames collapse,
			// (see https://github.com/dotnet/maui/issues/14418)
			// which is why we're ensuring that the Frame has at least the expected height/width

			Assert.True(layoutFrame.Height > minExpectedHeight);
			Assert.True(layoutFrame.Width > minExpectedWidth);
		}

		async Task<Rect> LayoutFrame(Layout layout, Frame frame, double widthConstraint, double heightConstraint, Func<Task> additionalTests = null)
		{
			additionalTests ??= () => Task.CompletedTask;
			return await
					AttachAndRun(layout, async (handler) =>
					{
						var size = (layout as IView).Measure(widthConstraint, heightConstraint);
						var rect = new Graphics.Rect(0, 0, size.Width, size.Height);
						(layout as IView).Arrange(rect);
						await OnFrameSetToNotEmpty(layout);
						await OnFrameSetToNotEmpty(frame);

						// verify that the PlatformView was measured
						var frameControlSize = (frame.Handler as IPlatformViewHandler).PlatformView.GetBoundingBox();
						Assert.True(frameControlSize.Width > 0);
						Assert.True(frameControlSize.Height > 0);

						// if the control sits inside a container make sure that also measured
						var containerControlSize = frame.ToPlatform().GetBoundingBox();
						Assert.True(containerControlSize.Width > 0);
						Assert.True(containerControlSize.Height > 0);

						await additionalTests.Invoke();
						return layout.Frame;
					});
		}
	}
}
