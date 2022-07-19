#nullable disable

using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.DirectX;
using Microsoft.UI.Composition;
using Windows.Foundation;
using Windows.Graphics.Imaging;

namespace Microsoft.Maui.Platform
{
	class CompositionImageBrush : IDisposable
	{
		CompositionGraphicsDevice _graphicsDevice;
		CompositionDrawingSurface _drawingSurface;
		CompositionSurfaceBrush _drawingBrush;

		public CompositionBrush Brush => _drawingBrush;

		public CompositionImageBrush()
		{
		}

		void CreateDevice(Compositor compositor)
		{
			_graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(
				compositor, CanvasDevice.GetSharedDevice());
		}

		void CreateDrawingSurface(global::Windows.Foundation.Size drawSize)
		{
			_drawingSurface = _graphicsDevice.CreateDrawingSurface(
				drawSize,
				DirectXPixelFormat.B8G8R8A8UIntNormalized,
				DirectXAlphaMode.Premultiplied);
		}

		void CreateSurfaceBrush(Compositor compositor)
		{
			_drawingBrush = compositor.CreateSurfaceBrush(_drawingSurface);
		}

		public static CompositionImageBrush FromBGRASoftwareBitmap(
			Compositor compositor,
			SoftwareBitmap bitmap,
			Size outputSize)
		{
			CompositionImageBrush brush = new CompositionImageBrush();

			brush.CreateDevice(compositor);

			brush.CreateDrawingSurface(outputSize);
			brush.DrawSoftwareBitmap(bitmap, outputSize);
			brush.CreateSurfaceBrush(compositor);

			return (brush);
		}

		void DrawSoftwareBitmap(SoftwareBitmap softwareBitmap, Size renderSize)
		{
			using (var drawingSession = CanvasComposition.CreateDrawingSession(_drawingSurface))
			using (var bitmap = CanvasBitmap.CreateFromSoftwareBitmap(drawingSession.Device, softwareBitmap))
			{
				drawingSession.DrawImage(bitmap,
					new Rect(0, 0, renderSize.Width, renderSize.Height));
			}
		}

		public void Dispose()
		{
			_drawingBrush.Dispose();
			_drawingSurface.Dispose();
			_graphicsDevice.Dispose();
		}
	}
}
