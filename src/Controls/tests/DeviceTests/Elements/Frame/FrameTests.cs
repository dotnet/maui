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
				await InvokeOnMainThreadAsync(async () =>
					await frame.ToPlatform(MauiContext).AttachAndRun(async () =>
					{
						(frame as IView).Measure(300, 300);
						(frame as IView).Arrange(new Graphics.Rect(0, 0, 300, 300));

						await OnFrameSetToNotEmpty(frame.Content);

						return frame.Content.Frame;

					})
				);


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
				platformView.AssertContainsColor(expectedColor);
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
				platformView.AssertContainsColor(expectedColor);
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

			var content = new Label { Text = "Hey", WidthRequest = 50, HeightRequest = 50 };

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

			Assert.True(layoutFrame.Width >= 52);
			Assert.True(layoutFrame.Height >= 52);
		}

		async Task<Rect> LayoutFrame(Layout layout, Frame frame, double measureWidth, double measureHeight)
		{
			return await InvokeOnMainThreadAsync(() =>
					layout.ToPlatform(MauiContext).AttachAndRun(async () =>
					{
						var size = (layout as IView).Measure(measureWidth, measureHeight);
						(layout as IView).Arrange(new Graphics.Rect(0, 0, size.Width, size.Height));

						await OnFrameSetToNotEmpty(layout);
						await OnFrameSetToNotEmpty(frame);

						// verify that the PlatformView was measured
						var frameControlSize = (frame.Handler as IPlatformViewHandler).PlatformView.GetBoundingBox();
						Assert.True(frameControlSize.Width > 0);
						Assert.True(frameControlSize.Width > 0);

						// if the control sits inside a container make sure that also measured
						var containerControlSize = frame.ToPlatform().GetBoundingBox();
						Assert.True(frameControlSize.Width > 0);
						Assert.True(frameControlSize.Width > 0);

						return layout.Frame;

					})
				);
		}
	}
}
