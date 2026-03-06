using System;
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
		float _logicalWidth;
		float _logicalHeight;
		float _adjustedScaleX;
		float _adjustedScaleY;

		public PlatformGraphicsView(Context context, IAttributeSet attrs, IDrawable drawable = null) : base(context, attrs)
		{
			_scale = Resources.DisplayMetrics.Density;
			_adjustedScaleX = _scale;
			_adjustedScaleY = _scale;
			_canvas = new PlatformCanvas(context);
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
		}

		public PlatformGraphicsView(Context context, IDrawable drawable = null) : base(context)
		{
			_scale = Resources.DisplayMetrics.Density;
			_adjustedScaleX = _scale;
			_adjustedScaleY = _scale;
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

			_canvas.Canvas = androidCanvas;
			if (_backgroundColor != null)
			{
				_canvas.FillColor = _backgroundColor;
				_canvas.FillRectangle(dirtyRect);
				_canvas.FillColor = Colors.White;
			}

			_scalingCanvas.ResetState();
			// Use adjusted scale factors that map rounded logical dp dimensions
			// back to exact pixel dimensions, avoiding both fractional dp values
			// and sub-pixel gaps at view edges.
			_scalingCanvas.Scale(_adjustedScaleX, _adjustedScaleY);
			dirtyRect.Width = _logicalWidth;
			dirtyRect.Height = _logicalHeight;
			_drawable.Draw(_scalingCanvas, dirtyRect);
			_canvas.Canvas = null;
		}

		protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
		{
			base.OnSizeChanged(width, height, oldWidth, oldHeight);
			_width = width;
			_height = height;
			UpdateLogicalDimensions();
		}

		/// <summary>
		/// Precomputes logical (dp) dimensions and adjusted scale factors.
		/// Rounding pixel÷density back to integer dp loses precision (e.g. 263/2.625 = 100.19).
		/// We round to the nearest integer dp, then derive an adjusted scale so that
		/// logicalDp × adjustedScale == exact pixel allocation — no fractional dp, no pixel gap.
		/// </summary>
		void UpdateLogicalDimensions()
		{
			_logicalWidth = MathF.Round(_width / _scale);
			_logicalHeight = MathF.Round(_height / _scale);

			if (_logicalWidth > 0 && _logicalHeight > 0)
			{
				_adjustedScaleX = _width / _logicalWidth;
				_adjustedScaleY = _height / _logicalHeight;
			}
			else
			{
				// Zero-size fallback: use device density as-is
				_adjustedScaleX = _scale;
				_adjustedScaleY = _scale;
				_logicalWidth = _width / _scale;
				_logicalHeight = _height / _scale;
			}
		}
	}
}
