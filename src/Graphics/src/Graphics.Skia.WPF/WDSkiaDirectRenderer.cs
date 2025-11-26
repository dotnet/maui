using System;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// A renderer that directly uses SkiaSharp to render graphics in a WPF application.
	/// </summary>
	/// <remarks>
	/// This class implements <see cref="ISkiaGraphicsRenderer"/> to provide drawing capabilities
	/// using SkiaSharp in WPF applications.
	/// </remarks>
	public class WDSkiaDirectRenderer : ISkiaGraphicsRenderer
	{
		private readonly SkiaCanvas _canvas;
		private readonly ScalingCanvas _scalingCanvas;
		private IDrawable _drawable;
		private WDSkiaGraphicsView _graphicsView;
		private Color _backgroundColor;

		/// <summary>
		/// Initializes a new instance of the <see cref="WDSkiaDirectRenderer"/> class.
		/// </summary>
		public WDSkiaDirectRenderer()
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
		}

		/// <summary>
		/// Gets the canvas used for drawing operations.
		/// </summary>
		public ICanvas Canvas => _scalingCanvas;

		/// <summary>
		/// Gets or sets the drawable that provides the content to be rendered.
		/// </summary>
		/// <remarks>
		/// When this property is set, the renderer is invalidated to reflect the new drawable.
		/// </remarks>
		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Sets the WPF graphics view associated with this renderer.
		/// </summary>
		public WDSkiaGraphicsView GraphicsView
		{
			set => _graphicsView = value;
		}

		/// <summary>
		/// Gets or sets the background color of the rendered content.
		/// </summary>
		public Color BackgroundColor
		{
			get => _backgroundColor;
			set => _backgroundColor = value;
		}

		/// <summary>
		/// Draws the content on the specified SkiaSharp canvas.
		/// </summary>
		/// <param name="skiaCanvas">The SkiaSharp canvas to draw on.</param>
		/// <param name="dirtyRect">The rectangle that needs to be redrawn.</param>
		/// <remarks>
		/// This method handles the actual drawing process, including setting up the canvas,
		/// drawing the background if needed, and invoking the drawable's draw method.
		/// </remarks>
		/// <exception cref="Exception">May throw an exception if the drawable encounters errors during drawing.</exception>
		public void Draw(
			SKCanvas skiaCanvas,
			RectF dirtyRect)
		{
			_canvas.Canvas = skiaCanvas;

			try
			{
				if (_backgroundColor != null)
				{
					_canvas.FillColor = _backgroundColor;
					_canvas.FillRectangle(dirtyRect);
					_canvas.FillColor = Colors.White;
				}

				_drawable.Draw(_scalingCanvas, dirtyRect);
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("An unexpected error occurred rendering the drawing: {0}", exc);
			}
			finally
			{
				_canvas.Canvas = null;
				_scalingCanvas.ResetState();
			}
		}

		/// <summary>
		/// Notifies the renderer that the size of the drawing surface has changed.
		/// </summary>
		/// <param name="width">The new width of the drawing surface.</param>
		/// <param name="height">The new height of the drawing surface.</param>
		/// <remarks>
		/// This implementation does not perform any actions when the size changes.
		/// </remarks>
		public void SizeChanged(
			int width,
			int height)
		{
			// Do nothing
		}

		/// <summary>
		/// Notifies the renderer that it has been detached from its view.
		/// </summary>
		/// <remarks>
		/// This implementation does not perform any actions when detached.
		/// </remarks>
		public void Detached()
		{
			// Do nothing
		}

		/// <summary>
		/// Releases all resources used by the renderer.
		/// </summary>
		/// <remarks>
		/// This implementation does not perform any actions when disposed.
		/// </remarks>
		public void Dispose()
		{
			// Do nothing
		}

		/// <summary>
		/// Invalidates the entire drawing surface, causing a redraw.
		/// </summary>
		/// <remarks>
		/// This method delegates to the associated graphics view's Invalidate method.
		/// </remarks>
		public void Invalidate()
		{
			_graphicsView?.Invalidate();
		}

		/// <summary>
		/// Invalidates a specific region of the drawing surface, causing that region to be redrawn.
		/// </summary>
		/// <param name="x">The x-coordinate of the region to invalidate.</param>
		/// <param name="y">The y-coordinate of the region to invalidate.</param>
		/// <param name="w">The width of the region to invalidate.</param>
		/// <param name="h">The height of the region to invalidate.</param>
		/// <remarks>
		/// This implementation currently ignores the specific region parameters and invalidates the entire view.
		/// </remarks>
		public void Invalidate(
			float x,
			float y,
			float w,
			float h)
		{
			_graphicsView?.Invalidate();
		}
	}
}
