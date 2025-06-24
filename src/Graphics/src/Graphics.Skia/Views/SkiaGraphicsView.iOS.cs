using System;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp.Views.iOS;

namespace Microsoft.Maui.Graphics.Skia.Views
{
	/// <summary>
	/// A SkiaSharp-based graphics view for iOS that can render <see cref="IDrawable"/> objects.
	/// </summary>
	public class SkiaGraphicsView : SKCanvasView
	{
		private IDrawable _drawable;
		private SkiaCanvas _canvas;
		private ScalingCanvas _scalingCanvas;

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaGraphicsView"/> class.
		/// </summary>
		/// <param name="drawable">The drawable object to render in this view.</param>
		public SkiaGraphicsView(IDrawable drawable = null)
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
		}

		/// <summary>
		/// Gets or sets the drawable object to render in this view.
		/// </summary>
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
		/// Invalidates the view and triggers a redraw.
		/// </summary>
		public void Invalidate()
		{
			if (Handle == IntPtr.Zero)
				return;

			SetNeedsDisplay();
		}

		protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			if (_drawable == null)
				return;

			var skiaCanvas = e.Surface.Canvas;
			skiaCanvas.Clear();

			var scale = (float)Window.Screen.Scale;
			_canvas.Canvas = skiaCanvas;

			_scalingCanvas.ResetState();
			_scalingCanvas.Scale(scale, scale);
			_drawable.Draw(_scalingCanvas, Bounds.AsRectangleF());
		}
	}
}
