using System;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Defines a renderer that uses SkiaSharp to render graphics in a WPF application.
	/// </summary>
	public interface ISkiaGraphicsRenderer : IDisposable
	{
		/// <summary>
		/// Gets or sets the associated WPF Skia graphics view.
		/// </summary>
		WDSkiaGraphicsView GraphicsView { set; }

		/// <summary>
		/// Gets the canvas used for drawing operations.
		/// </summary>
		ICanvas Canvas { get; }

		/// <summary>
		/// Gets or sets the drawable that provides the content to be rendered.
		/// </summary>
		IDrawable Drawable { get; set; }

		/// <summary>
		/// Gets or sets the background color of the rendered content.
		/// </summary>
		Color BackgroundColor { get; set; }

		/// <summary>
		/// Draws the content on the specified SkiaSharp canvas.
		/// </summary>
		/// <param name="canvas">The SkiaSharp canvas to draw on.</param>
		/// <param name="dirtyRect">The rectangle that needs to be redrawn.</param>
		void Draw(SKCanvas canvas, RectF dirtyRect);

		/// <summary>
		/// Notifies the renderer that the size of the drawing surface has changed.
		/// </summary>
		/// <param name="width">The new width of the drawing surface.</param>
		/// <param name="height">The new height of the drawing surface.</param>
		void SizeChanged(int width, int height);

		/// <summary>
		/// Notifies the renderer that it has been detached from its view.
		/// </summary>
		void Detached();

		/// <summary>
		/// Invalidates the entire drawing surface, causing a redraw.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Invalidates a specific region of the drawing surface, causing that region to be redrawn.
		/// </summary>
		/// <param name="x">The x-coordinate of the region to invalidate.</param>
		/// <param name="y">The y-coordinate of the region to invalidate.</param>
		/// <param name="w">The width of the region to invalidate.</param>
		/// <param name="h">The height of the region to invalidate.</param>
		void Invalidate(float x, float y, float w, float h);
	}
}
