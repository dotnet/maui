using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformGraphicsView : View
	{
		private int _width, _height;
		private readonly PlatformCanvas _canvas;
		private readonly ScalingCanvas _scalingCanvas;
		private IDrawable _drawable;
		private readonly float _scale = 1;
		private Color _backgroundColor;

		public PlatformGraphicsView(Context context, IAttributeSet attrs, IDrawable drawable = null) : base(context, attrs)
		{
			_scale = Resources.DisplayMetrics.Density;
			_canvas = new PlatformCanvas(context);
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
		}

		public PlatformGraphicsView(Context context, IDrawable drawable = null) : base(context)
		{
			_scale = Resources.DisplayMetrics.Density;
			_canvas = new PlatformCanvas(context);
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
		}

		public Color BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value;
				Invalidate();
			}
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

		public override void Draw(Canvas androidCanvas)
		{
			if (_drawable == null)
				return;

			var dirtyRect = new RectF(0, 0, _width, _height);

			// To draw within the canvas bounds
			androidCanvas.Save();
			androidCanvas.ClipRect(androidCanvas.ClipBounds);

			_canvas.Canvas = androidCanvas;
			if (_backgroundColor != null)
			{
				_canvas.FillColor = _backgroundColor;
				_canvas.FillRectangle(dirtyRect);
				_canvas.FillColor = Colors.White;
			}

			_scalingCanvas.ResetState();
			_scalingCanvas.Scale(_scale, _scale);
			//Since we are using a scaling canvas, we need to scale the rectangle
			dirtyRect.Height /= _scale;
			dirtyRect.Width /= _scale;
			_drawable.Draw(_scalingCanvas, dirtyRect);

			androidCanvas.Restore();
			_canvas.Canvas = null;
		}

		protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
		{
			base.OnSizeChanged(width, height, oldWidth, oldHeight);
			_width = width;
			_height = height;
		}
	}
}
