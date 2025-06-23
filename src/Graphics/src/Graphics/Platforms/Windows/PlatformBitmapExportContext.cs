using System;
using System.IO;
using Microsoft.Graphics.Canvas;
using Windows.Storage.Streams;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform implementation of <see cref="BitmapExportContext"/> using Win2D.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
	public class W2DBitmapExportContext
#else
	public class PlatformBitmapExportContext
#endif
		: BitmapExportContext
	{
		private readonly ICanvasResourceCreator _creator;
		private readonly CanvasRenderTarget _renderTarget;
		private readonly CanvasDrawingSession _session;
		private PlatformCanvas _canvas;

#if MAUI_GRAPHICS_WIN2D
		public W2DBitmapExportContext(
#else
		public PlatformBitmapExportContext(
#endif
			int width, int height, float displayScale = 1, int dpi = 96) : base(width, height, dpi)
		{
			_creator = PlatformGraphicsService.Creator;
			if (_creator == null)
			{
				throw new InvalidOperationException("No resource creator has been registered globally or for this thread.");
			}

			_renderTarget = new CanvasRenderTarget(_creator, width, height, dpi);
			_session = _renderTarget.CreateDrawingSession();
			
			_canvas = new PlatformCanvas()
			{
				CanvasSize = new Windows.Foundation.Size(width, height),
				DisplayScale = displayScale,
				Session = _session
			};
		}

		public override ICanvas Canvas => _canvas;

		public CanvasRenderTarget RenderTarget => _renderTarget;

#if MAUI_GRAPHICS_WIN2D
		public W2DImage W2DImage => new W2DImage(_creator, _renderTarget);
#else
		public PlatformImage PlatformImage => new PlatformImage(_creator, _renderTarget);
#endif

		public override IImage Image => PlatformImage;

		public override void WriteToStream(Stream stream)
		{
			using var randomAccessStream = new InMemoryRandomAccessStream();
			
			// Save the render target to a stream
			var saveTask = _renderTarget.SaveAsync(randomAccessStream, CanvasBitmapFileFormat.Png);
			saveTask.AsTask().GetAwaiter().GetResult();
			
			// Copy to the output stream
			randomAccessStream.Seek(0);
			randomAccessStream.AsStreamForRead().CopyTo(stream);
		}

		public override void Dispose()
		{
			_session?.Dispose();
			_renderTarget?.Dispose();
			_canvas?.Dispose();
			base.Dispose();
		}
	}
}
