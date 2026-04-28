using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using ALayoutDirection = Android.Views.LayoutDirection;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformGraphicsView : View
	{
		private int _width, _height;
		private readonly PlatformCanvas _canvas;
		private readonly ScalingCanvas _scalingCanvas;
		private IDrawable _drawable;
		private float _scale;
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

		// Overridden by friend assemblies to supply the density source used for draw scaling.
		internal virtual float GetDisplayDensity() => Resources.DisplayMetrics.Density;

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

			if (LayoutDirection == ALayoutDirection.Rtl)
			{
				int save = androidCanvas.Save();
				androidCanvas.Translate(Width, 0);
				androidCanvas.Scale(-1, 1);
				try
				{
					DrawContent(androidCanvas);
				}
				finally
				{
					androidCanvas.RestoreToCount(save);
				}
			}
			else
			{
				DrawContent(androidCanvas);
			}
		}

		void DrawContent(Canvas androidCanvas)
		{
			var dirtyRect = new RectF(0, 0, _width, _height);

			// Save the canvas state and clip to view bounds to prevent drawing outside
			androidCanvas.Save();
			androidCanvas.ClipRect(0, 0, _width, _height);

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
			_scale = GetDisplayDensity();
			_width = width;
			_height = height;
		}
	}
}
