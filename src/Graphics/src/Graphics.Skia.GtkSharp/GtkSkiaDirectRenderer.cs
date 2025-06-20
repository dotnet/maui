using System;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// A Skia graphics renderer that directly renders to a GTK surface.
	/// </summary>
	public class GtkSkiaDirectRenderer : ISkiaGraphicsRenderer
	{
		private readonly SkiaCanvas _canvas;
		private readonly ScalingCanvas _scalingCanvas;
		private IDrawable _drawable;
		private GtkSkiaGraphicsView _graphicsView;
		private Color _backgroundColor;

		/// <summary>
		/// Initializes a new instance of the <see cref="GtkSkiaDirectRenderer"/> class.
		/// </summary>
		public GtkSkiaDirectRenderer()
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
		}

		/// <summary>
		/// Gets the canvas used for drawing operations.
		/// </summary>
		/// <value>The scaling canvas wrapping the Skia canvas.</value>
		public ICanvas Canvas => _scalingCanvas;

		/// <summary>
		/// Gets or sets the drawable object that will be rendered.
		/// </summary>
		/// <value>The drawable content to be rendered on the canvas.</value>
		/// <remarks>
		/// When the drawable is changed, the view is automatically invalidated to trigger a redraw.
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
		/// Sets the GTK Skia graphics view associated with this renderer.
		/// </summary>
		/// <value>The graphics view that this renderer will update.</value>
		public GtkSkiaGraphicsView GraphicsView
		{
			set => _graphicsView = value;
		}

		/// <summary>
		/// Gets or sets the background color for the rendered content.
		/// </summary>
		/// <value>The color used to fill the background of the canvas.</value>
		public Color BackgroundColor
		{
			get => _backgroundColor;
			set => _backgroundColor = value;
		}

		/// <summary>
		/// Draws the content to the specified Skia canvas within the dirty rectangle.
		/// </summary>
		/// <param name="skiaCanvas">The Skia canvas to draw on.</param>
		/// <param name="dirtyRect">The rectangle region that needs to be redrawn.</param>
		/// <exception cref="Exception">Any exceptions that occur during the drawing process are caught and logged.</exception>
		public void Draw(
			SKCanvas skiaCanvas,
			RectF dirtyRect)
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
				System.Diagnostics.Debug.WriteLine("An unexpected error occurred rendering the drawing: {0}", exc);
			}
			finally
			{
				_canvas.Canvas = null;
				_scalingCanvas.ResetState();
			}
		}

		/// <summary>
		/// Notifies the renderer that the size of its container has changed.
		/// </summary>
		/// <param name="width">The new width of the container.</param>
		/// <param name="height">The new height of the container.</param>
		/// <remarks>This implementation does not perform any actions when the size changes.</remarks>
		public void SizeChanged(
			int width,
			int height)
		{
			// Do nothing
		}

		/// <summary>
		/// Notifies the renderer that it has been detached from its container.
		/// </summary>
		/// <remarks>This implementation does not perform any actions when detached.</remarks>
		public void Detached()
		{
			// Do nothing
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>This implementation does not release any resources.</remarks>
		public void Dispose()
		{
			// Do nothing
		}

		/// <summary>
		/// Invalidates the entire drawing surface, causing a redraw.
		/// </summary>
		/// <remarks>
		/// Calls <see cref="Gtk.Widget.QueueDraw"/> on the associated <see cref="GtkSkiaGraphicsView"/> to request a redraw.
		/// </remarks>
		public void Invalidate()
		{
			_graphicsView?.QueueDraw();
		}

		/// <summary>
		/// Invalidates a specific rectangle area of the drawing surface, causing that region to be redrawn.
		/// </summary>
		/// <param name="x">The x-coordinate of the top-left corner of the rectangle to invalidate.</param>
		/// <param name="y">The y-coordinate of the top-left corner of the rectangle to invalidate.</param>
		/// <param name="w">The width of the rectangle to invalidate.</param>
		/// <param name="h">The height of the rectangle to invalidate.</param>
		/// <remarks>
		/// Calls <see cref="Gtk.Widget.QueueDrawArea"/> on the associated <see cref="GtkSkiaGraphicsView"/> to request a redraw of the specified area.
		/// </remarks>
		public void Invalidate(
			float x,
			float y,
			float w,
			float h)
		{
			_graphicsView?.QueueDrawArea((int)x, (int)y, (int)w, (int)h);
		}

	}
}
