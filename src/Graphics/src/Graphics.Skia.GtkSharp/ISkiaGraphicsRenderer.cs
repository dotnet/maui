using System;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Defines a renderer for Skia graphics that can be used with GTK.
	/// </summary>
	public interface ISkiaGraphicsRenderer : IDisposable
	{
		/// <summary>
		/// Gets or sets the GTK Skia graphics view associated with this renderer.
		/// </summary>
		/// <value>The graphics view that this renderer will update.</value>
		GtkSkiaGraphicsView GraphicsView { set; }

		/// <summary>
		/// Gets the canvas used for drawing operations.
		/// </summary>
		/// <value>The canvas instance used for rendering graphics.</value>
		ICanvas Canvas { get; }

		/// <summary>
		/// Gets or sets the drawable object that will be rendered.
		/// </summary>
		/// <value>The drawable content to be rendered on the canvas.</value>
		IDrawable Drawable { get; set; }

		/// <summary>
		/// Gets or sets the background color for the rendered content.
		/// </summary>
		/// <value>The color used to fill the background of the canvas.</value>
		Color BackgroundColor { get; set; }

		/// <summary>
		/// Draws the content to the specified Skia canvas within the dirty rectangle.
		/// </summary>
		/// <param name="canvas">The Skia canvas to draw on.</param>
		/// <param name="dirtyRect">The rectangle region that needs to be redrawn.</param>
		void Draw(SKCanvas canvas, RectF dirtyRect);

		/// <summary>
		/// Notifies the renderer that the size of its container has changed.
		/// </summary>
		/// <param name="width">The new width of the container.</param>
		/// <param name="height">The new height of the container.</param>
		void SizeChanged(int width, int height);

		/// <summary>
		/// Notifies the renderer that it has been detached from its container.
		/// </summary>
		void Detached();

		/// <summary>
		/// Invalidates the entire drawing surface, causing a redraw.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Invalidates a specific rectangle area of the drawing surface, causing that region to be redrawn.
		/// </summary>
		/// <param name="x">The x-coordinate of the top-left corner of the rectangle to invalidate.</param>
		/// <param name="y">The y-coordinate of the top-left corner of the rectangle to invalidate.</param>
		/// <param name="w">The width of the rectangle to invalidate.</param>
		/// <param name="h">The height of the rectangle to invalidate.</param>
		void Invalidate(float x, float y, float w, float h);
	}
}
