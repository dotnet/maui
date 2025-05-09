using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp.Views.Tizen.NUI;
using Tizen.NUI;
using SKPaintSurfaceEventArgs = SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs;

namespace Microsoft.Maui.Graphics.Skia.Views
{
	/// <summary>
	/// A SkiaSharp-based graphics view for Tizen that can render <see cref="IDrawable"/> objects.
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
		public SkiaGraphicsView(IDrawable drawable = null) : base()
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
			PaintSurface += OnPaintSurface;
			IgnorePixelScaling = true;
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
		/// Handles the paint surface event to draw the content.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments containing the surface to draw on.</param>
		protected virtual void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			if (_drawable == null)
				return;

			var skiaCanvas = e.Surface.Canvas;
			skiaCanvas.Clear();

			_canvas.Canvas = skiaCanvas;
			_drawable.Draw(_scalingCanvas, new RectF(0, 0, e.Info.Width, e.Info.Height));
		}
	}
}
