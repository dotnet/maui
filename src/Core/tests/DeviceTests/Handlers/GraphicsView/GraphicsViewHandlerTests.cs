﻿using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using Microsoft.Maui.Storage;

#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
using PlatformImageLoadingService = Microsoft.Maui.Graphics.Win2D.W2DImageLoadingService;
#else
using Microsoft.Maui.Graphics.Platform;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.GraphicsView)]
	public partial class GraphicsViewHandlerTests : CoreHandlerTestBase<GraphicsViewHandler, GraphicsViewStub>
	{
		[Theory(DisplayName = "GraphicsView Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task GraphicsViewInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var graphicsView = new GraphicsViewStub()
			{
				Drawable = new TestDrawable(expected),
				Height = 100,
				Width = 200
			};

			await ValidateHasColor(graphicsView, expected);
		}

		[Theory(DisplayName = "GraphicsView Can Draw Image Correctly")]
		[InlineData("red.png", 0xFFFF0000)]
		[InlineData("green.png", 0xFF00FF00)]
		[InlineData("blue.png", 0xFF0000FF)]
		[InlineData("white.png", 0xFFFFFFFF)]
		[InlineData("black.png", 0xFF000000)]
		public async Task GraphicsViewCanDrawImage(string filename, uint color)
		{
			var expected = Color.FromUint(color);

			var image = await Task.Run(async () =>
			{
				var loading = new PlatformImageLoadingService();
				using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
				return loading.FromStream(stream);
			});

			var graphicsView = new GraphicsViewStub()
			{
				Drawable = new TestDrawable(image),
				Height = 100,
				Width = 200
			};

			await ValidateHasColor(graphicsView, expected);
		}
	}

	public class TestDrawable : IDrawable
	{
		public TestDrawable(Color fillColor)
		{
			FillColor = fillColor;
		}

		public TestDrawable(Graphics.IImage image)
		{
			Image = image;
		}

		public Color FillColor { get; set; }

		public Graphics.IImage Image { get; set; }

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			if (FillColor is not null)
			{
				canvas.FillColor = FillColor;
				canvas.FillRectangle(dirtyRect);
			}

			if (Image is not null)
			{
				canvas.DrawImage(Image, dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
			}
		}
	}
}