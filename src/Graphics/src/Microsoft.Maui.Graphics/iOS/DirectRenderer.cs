using System;
using System.Drawing;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public class DirectRenderer : IGraphicsRenderer
	{
		private readonly PlatformCanvas _canvas;
		private IDrawable _drawable;
		private PlatformGraphicsView _graphicsView;

		public DirectRenderer()
		{
			_canvas = new PlatformCanvas(() => CGColorSpace.CreateDeviceRGB());
		}

		public ICanvas Canvas => _canvas;

		public IDrawable Drawable
		{
			get => _drawable;
			set => _drawable = value;
		}

		public PlatformGraphicsView GraphicsView
		{
			set => _graphicsView = value;
		}

		public void Draw(CGContext coreGraphics, RectangleF dirtyRect)
		{
			_canvas.Context = coreGraphics;

			try
			{
				_drawable.Draw(_canvas, dirtyRect);
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("An unexpected error occurred rendering the drawing: {0}", exc);
			}
			finally
			{
				_canvas.Context = null;
			}
		}

		public void SizeChanged(float width, float height)
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
			_graphicsView?.SetNeedsDisplay();
		}

		public void Invalidate(float x, float y, float w, float h)
		{
			_graphicsView?.SetNeedsDisplayInRect(new RectangleF(x, y, w, h));
		}
	}
}
