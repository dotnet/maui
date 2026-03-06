using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class GraphicsViewHandlerTests
	{
		PlatformGraphicsView GetPlatformGraphicsView(GraphicsViewHandler graphicsViewHandler) =>
			graphicsViewHandler.PlatformView;

		[Fact(DisplayName = "GraphicsView dirtyRect dimensions are integer values on Android")]
		public async Task GraphicsViewDirtyRectHasIntegerDimensionsOnAndroid()
		{
			var drawable = new DirtyRectCapturingDrawable();
			var graphicsView = new GraphicsView
			{
				Drawable = drawable,
				WidthRequest = 100,
				HeightRequest = 50
			};

			await AttachAndRun(graphicsView, async (handler) =>
			{
				await Task.Delay(200); // wait for layout pass

				Assert.NotNull(drawable.CapturedDirtyRect);
				var rect = drawable.CapturedDirtyRect!.Value;

				Assert.True(
					Math.Abs(rect.Width - Math.Round(rect.Width)) < 0.01,
					$"dirtyRect.Width ({rect.Width}) should be an integer value");
				Assert.True(
					Math.Abs(rect.Height - Math.Round(rect.Height)) < 0.01,
					$"dirtyRect.Height ({rect.Height}) should be an integer value");
			});
		}

		class DirtyRectCapturingDrawable : IDrawable
		{
			public RectF? CapturedDirtyRect { get; private set; }

			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				CapturedDirtyRect = dirtyRect;
			}
		}
	}
}