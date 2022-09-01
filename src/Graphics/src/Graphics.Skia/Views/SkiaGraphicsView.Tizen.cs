using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp.Views.Tizen.NUI;
using Tizen.NUI;
using SKPaintSurfaceEventArgs = SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs;

namespace Microsoft.Maui.Graphics.Skia.Views
{
	public class SkiaGraphicsView : SKCanvasView
	{
		private IDrawable _drawable;
		private SkiaCanvas _canvas;
		private ScalingCanvas _scalingCanvas;

		public SkiaGraphicsView(IDrawable drawable = null) : base()
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
			PaintSurface += OnPaintSurface;
			IgnorePixelScaling = true;
		}

		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				Invalidate();
			}
		}

		protected virtual void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			if (_drawable == null) return;

			var skiaCanvas = e.Surface.Canvas;
			skiaCanvas.Clear();

			_canvas.Canvas = skiaCanvas;
			_drawable.Draw(_scalingCanvas, new RectF(0, 0, e.Info.Width , e.Info.Height));
		}
	}
}
