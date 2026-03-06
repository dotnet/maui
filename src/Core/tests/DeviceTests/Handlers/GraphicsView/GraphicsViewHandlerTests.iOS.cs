using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class GraphicsViewHandlerTests
	{
		PlatformGraphicsView GetPlatformGraphicsView(GraphicsViewHandler graphicsViewHandler) =>
			graphicsViewHandler.PlatformView;

		// Regression test for https://github.com/dotnet/maui/issues/31239
		// Verifies that setting Background on GraphicsView applies the color to the platform layer.
		[Fact(DisplayName = "GraphicsView applies Background color to platform layer")]
		public async Task GraphicsViewAppliesBackgroundToLayer()
		{
			var expectedColor = Colors.Red;

			var graphicsView = new GraphicsViewStub
			{
				Drawable = new TestDrawable(Colors.Blue),
				Background = new SolidPaintStub(expectedColor),
				Width = 100,
				Height = 100,
			};

			var platformBackgroundColor = await GetValueAsync(graphicsView, (GraphicsViewHandler handler) =>
			{
				var nativeView = GetPlatformGraphicsView(handler);
				return nativeView.BackgroundColor;
			});

			Assert.NotNull(platformBackgroundColor);

			platformBackgroundColor.GetRGBA(out var r, out var g, out var b, out var a);

			Assert.Equal(expectedColor.Red, (float)r, 2);
			Assert.Equal(expectedColor.Green, (float)g, 2);
			Assert.Equal(expectedColor.Blue, (float)b, 2);
		}

		// Regression test for https://github.com/dotnet/maui/issues/25502
		// A GraphicsView with decimal WidthRequest/HeightRequest must have pixel-aligned Bounds
		// to prevent the CALayer background from rendering a gray hairline at the sub-pixel edge.
		[Fact(DisplayName = "GraphicsView Bounds are pixel-aligned when decimal dimensions are used")]
		public async Task GraphicsViewBoundsArePixelAlignedWithDecimalDimensions()
		{
			var graphicsView = new GraphicsViewStub
			{
				Drawable = new TestDrawable(Colors.White),
				Background = new SolidPaintStub(Colors.White),
				Width = 248.25,
				Height = 200.25,
			};

			var (bounds, scale) = await GetValueAsync(graphicsView, (GraphicsViewHandler handler) =>
			{
				var nativeView = GetPlatformGraphicsView(handler);
				return (nativeView.Bounds, (double)UIScreen.MainScreen.Scale);
			}, attachAndRun: true);

			var widthInPixels = bounds.Width * scale;
			var heightInPixels = bounds.Height * scale;

			Assert.True(
				Math.Abs(widthInPixels - Math.Round(widthInPixels)) < 0.01,
				$"GraphicsView Bounds.Width ({bounds.Width}pt) is not pixel-aligned on a {scale}x display. " +
				$"Physical width {widthInPixels}px must be an integer to prevent the gray hairline artifact.");

			Assert.True(
				Math.Abs(heightInPixels - Math.Round(heightInPixels)) < 0.01,
				$"GraphicsView Bounds.Height ({bounds.Height}pt) is not pixel-aligned on a {scale}x display. " +
				$"Physical height {heightInPixels}px must be an integer to prevent the gray hairline artifact.");
		}
	}
}