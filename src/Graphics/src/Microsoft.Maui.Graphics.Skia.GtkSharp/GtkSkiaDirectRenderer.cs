using SkiaSharp;
using System;

namespace Microsoft.Maui.Graphics.Skia
{
	public class GtkSkiaDirectRenderer : ISkiaGraphicsRenderer
	{
		private readonly SkiaCanvas _canvas;
		private readonly ScalingCanvas _scalingCanvas;
		private IDrawable _drawable;
		private GtkSkiaGraphicsView _graphicsView;
		private Color _backgroundColor;

		public GtkSkiaDirectRenderer()
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
		}

		public ICanvas Canvas => _scalingCanvas;

		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				Invalidate();
			}
		}

		public GtkSkiaGraphicsView GraphicsView
		{
			set => _graphicsView = value;
		}

		public Color BackgroundColor
		{
			get => _backgroundColor;
			set => _backgroundColor = value;
		}

		public void Draw(
			SKCanvas skiaCanvas,
			RectangleF dirtyRect)
		{
			_canvas.Canvas = skiaCanvas;

			try
			{
				_canvas.SaveState();

				if (_backgroundColor != null)
				{
					_canvas.FillColor = _backgroundColor;
					_canvas.FillRectangle(dirtyRect);
				}
				else
				{
					_canvas.ClipRectangle(dirtyRect);
					_canvas.Canvas.Clear();
				}

				_canvas.RestoreState();

				_drawable.Draw(_scalingCanvas, dirtyRect);
			}
			catch (Exception exc)
			{
				Logger.Error("An unexpected error occurred rendering the drawing.", exc);
			}
			finally
			{
				_canvas.Canvas = null;
				_scalingCanvas.ResetState();
			}
		}

		public void SizeChanged(
			int width,
			int height)
		{
			// Do nothing
		}

		public void Detached()
		{
			// Do nothing
		}

		public void Dispose()
		{
			// Do nothing
		}

		public void Invalidate()
		{
			_graphicsView?.QueueDraw();
		}

		public void Invalidate(
			float x,
			float y,
			float w,
			float h) {
			_graphicsView?.QueueDrawArea ((int) x, (int) y, (int) w, (int) h);
		}

	}
}
