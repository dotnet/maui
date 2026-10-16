using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class GraphicsViewHandlerTests
	{
		PlatformTouchGraphicsView GetPlatformGraphicsView(GraphicsViewHandler graphicsViewHandler) =>
			graphicsViewHandler.PlatformView;

		[Fact(DisplayName = "GraphicsView releases Win2D drawing sessions after repeated drawing")]
		public async Task GraphicsViewReleasesWin2DDrawingSessionsAfterRepeatedDrawing()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var drawable = new CapturingDrawable();
				var graphicsView = new W2DGraphicsView
				{
					Width = 100,
					Height = 100,
					Drawable = drawable,
				};

				await graphicsView.AttachAndRun(
					async () =>
					{
						var canvas = await drawable.NextDraw.WaitAsync(TimeSpan.FromSeconds(5));
						Assert.Null(canvas.Session);

						for (int i = 0; i < 100; i++)
						{
							var nextDraw = drawable.PrepareNextDraw();
							graphicsView.Invalidate();
							canvas = await nextDraw.WaitAsync(TimeSpan.FromSeconds(5));

							Assert.Null(canvas.Session);
						}
					},
					MauiContext);
			});
		}

		sealed class CapturingDrawable : IDrawable
		{
			TaskCompletionSource<W2DCanvas> _nextDraw = CreateCompletionSource();

			public Task<W2DCanvas> NextDraw => _nextDraw.Task;

			public Task<W2DCanvas> PrepareNextDraw()
			{
				_nextDraw = CreateCompletionSource();
				return _nextDraw.Task;
			}

			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				_nextDraw.TrySetResult(Assert.IsType<W2DCanvas>(canvas));
			}

			static TaskCompletionSource<W2DCanvas> CreateCompletionSource() =>
				new(TaskCreationOptions.RunContinuationsAsynchronously);
		}
	}
}