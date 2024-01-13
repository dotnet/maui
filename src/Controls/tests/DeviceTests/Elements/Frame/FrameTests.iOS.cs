using System.Threading.Tasks;
using CoreAnimation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FrameTests
	{
		[Fact(DisplayName = "Frame with gradient Background Test")]
		public async Task FrameWithGradientBackgroundTest()
		{
			SetupBuilder();

			var frame = new Frame()
			{
				HasShadow = false,
				HeightRequest = 200,
				WidthRequest = 200,
				Background = new LinearGradientBrush
				{
					StartPoint = new Point(0, 0),
					EndPoint = new Point(0, 1),
					GradientStops = new GradientStopCollection
					{
						new GradientStop { Color = Colors.Transparent, Offset = 0 },
						new GradientStop { Color = Colors.Red, Offset = 0.3f },
						new GradientStop { Color = Colors.Transparent, Offset = 0.6f },
						new GradientStop { Color = Colors.Red, Offset = 1 },
					}
				},
				Content = new Label()
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Text = "Gradient Background"
				}
			};

			await InvokeOnMainThreadAsync(() =>
				frame.ToPlatform(MauiContext).AttachAndRun(() =>
				{
					var platformView = (Controls.Handlers.Compatibility.FrameRenderer)frame.ToPlatform(MauiContext);
					Assert.NotNull(platformView);

					var backgroundLayer = GetBackgroundLayer(platformView) as CAGradientLayer;
					Assert.NotNull(backgroundLayer);

					var backgroundLayerColors = backgroundLayer.Colors;
					Assert.Equal(4, backgroundLayerColors.Length);
					string transparentColor = "transparent";
					Assert.Equal(transparentColor, backgroundLayerColors[0].AXName);
					Assert.Equal(transparentColor, backgroundLayerColors[2].AXName);
				}));
		}

		[Fact(DisplayName = "Frame HasShadow Test")]
		public async Task FrameHasShadowTest()
		{
			SetupBuilder();

			var frame = new Frame()
			{
				HasShadow = true,
				HeightRequest = 200,
				WidthRequest = 200,
				Content = new Label()
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Text = "HasShadow"
				}
			};

			await InvokeOnMainThreadAsync(() =>
				frame.ToPlatform(MauiContext).AttachAndRun(async () =>
				{
					var platformView = (Controls.Handlers.Compatibility.FrameRenderer)frame.ToPlatform(MauiContext);
					Assert.NotNull(platformView);

					// The way the shadow is applied in .NET MAUI on iOS is the same way it was applied in Forms
					// so on iOS we just return the shadow that was hard coded into the renderer
					var expectedShadow = new Shadow() { Radius = 5, Opacity = 0.8f, Offset = new Point(0, 0), Brush = Brush.Black };

					if (platformView.Element is IView element)
					{
						var platformShadow = element.Shadow;
						await AssertEventually(() => platformShadow != null);

						Assert.Equal(platformShadow.Radius, expectedShadow.Radius);
						Assert.Equal(platformShadow.Opacity, expectedShadow.Opacity);
						Assert.Equal(platformShadow.Offset, expectedShadow.Offset);
					}
				}));
		}

		[Theory(DisplayName = "Frame's Content Clips to Bounds Properly")]
		[InlineData(true)]
		[InlineData(false)]
		[InlineData(null)]
		public async Task FrameClipsCorrectly(bool? isClipped)
		{
			SetupBuilder();

			var frame = new Frame()
			{
				HeightRequest = 300,
				WidthRequest = 300,
				CornerRadius = 80,
				Content = new Frame
				{
					HeightRequest = 400,
					WidthRequest = 400,
					BackgroundColor = Colors.Blue,
				}
			};

			if (isClipped is bool clipped)
				frame.IsClippedToBounds = clipped!;

			await InvokeOnMainThreadAsync(() =>
				frame.ToPlatform(MauiContext).AttachAndRun(() =>
				{
					var handler = frame.ToHandler(MauiContext);
					if (isClipped == false)
						Assert.False(handler.PlatformView.ClipsToBounds);
					else
						Assert.True(handler.PlatformView.ClipsToBounds);
				}));
		}

		CALayer GetBackgroundLayer(UIView platformView)
		{
			string BackgroundLayer = "BackgroundLayer";

			var layer = platformView.Layer;

			if (layer is not null)
			{
				if (layer.Name == BackgroundLayer)
					return layer;

				if (layer.Sublayers == null || layer.Sublayers.Length == 0)
					return null;

				foreach (var subLayer in layer.Sublayers)
				{
					if (subLayer.Name == BackgroundLayer)
						return subLayer;
				}
			}

			return null;
		}
	}
}
