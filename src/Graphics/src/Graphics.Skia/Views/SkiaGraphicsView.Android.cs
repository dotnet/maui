using Android.App;
using Android.Content;
using Android.Util;
using Microsoft.Maui.Graphics.Android;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp.Views.Android;

namespace Microsoft.Maui.Graphics.Skia.Views
{
	/// <summary>
	/// A SkiaSharp-based graphics view for Android that can render <see cref="IDrawable"/> objects.
	/// </summary>
	public class SkiaGraphicsView : SKCanvasView
	{
		private IDrawable _drawable;
		private SkiaCanvas _canvas;
		private ScalingCanvas _scalingCanvas;
		private float _width, _height;
		private readonly float _scale = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaGraphicsView"/> class.
		/// </summary>
		/// <param name="context">The Android context.</param>
		/// <param name="drawable">The drawable object to render in this view.</param>
		public SkiaGraphicsView(Context context, IDrawable drawable = null) : base(context)
		{
			_scale = Resources.DisplayMetrics.Density;
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

		protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			if (_drawable == null)
				return;

			var skiaCanvas = e.Surface.Canvas;
			skiaCanvas.Clear();

			_canvas.Canvas = skiaCanvas;

			_scalingCanvas.ResetState();
			_scalingCanvas.Scale(_scale, _scale);

			_drawable.Draw(_scalingCanvas, new RectF(0, 0, _width / _scale, _height / _scale));
		}

		protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
		{
			base.OnSizeChanged(width, height, oldWidth, oldHeight);
			_width = width;
			_height = height;
		}
	}
}
