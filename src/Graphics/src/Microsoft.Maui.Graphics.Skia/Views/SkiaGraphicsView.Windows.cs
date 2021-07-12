using Microsoft.UI.Xaml;
using SkiaSharp.Views.Windows;

namespace Microsoft.Maui.Graphics.Skia.Views
{
	public class SkiaGraphicsView : SKXamlCanvas
	{
		private IDrawable _drawable;
		private SkiaCanvas _canvas;
		private ScalingCanvas _scalingCanvas;
		private float _width, _height;

		public SkiaGraphicsView(IDrawable drawable = null)
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;

			SizeChanged += OnSizeChanged;
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

		protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			if (_drawable == null) return;

			var scale = (float)Dpi;

			var skiaCanvas = e.Surface.Canvas;
			skiaCanvas.Clear();

			_canvas.Canvas = skiaCanvas;

			_scalingCanvas.ResetState();
			_scalingCanvas.Scale(scale, scale);

			_drawable.Draw(_scalingCanvas, new RectangleF(0, 0, _width / scale, _height / scale));
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			_width = (float)e.NewSize.Width;
			_height = (float)e.NewSize.Height;
		}
	}
}
