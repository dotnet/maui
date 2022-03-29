#nullable disable
using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using APath = Android.Graphics.Path;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : ViewGroup
	{
		const int MaximumRadius = 100;

		readonly Android.Graphics.Rect _viewBounds;

		APath _currentPath;
		SizeF _lastPathSize; 
		bool _invalidateClip;

		Bitmap _shadowBitmap;
		Canvas _shadowCanvas;
		Android.Graphics.Paint _shadowPaint;
		bool _invalidateShadow;

		AView _borderView;

		public bool InputTransparent { get; set; }

		public WrapperView(Context context)
			: base(context)
		{
			_viewBounds = new Android.Graphics.Rect();

			SetClipChildren(false);
			SetWillNotDraw(true);
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();

			_invalidateShadow = true;

			if (_shadowBitmap != null)
			{
				_shadowBitmap.Recycle();
				_shadowBitmap = null;
			}
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			_borderView?.BringToFront();

			if (ChildCount == 0 || GetChildAt(0) is not AView child)
				return;

			var widthMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(right - left);
			var heightMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(bottom - top);

			child.Measure(widthMeasureSpec, heightMeasureSpec);
			child.Layout(0, 0, child.MeasuredWidth, child.MeasuredHeight);
			_borderView?.Layout(0, 0, child.MeasuredWidth, child.MeasuredHeight);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (ChildCount == 0 || GetChildAt(0) is not View child)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				return;
			}

			_viewBounds.Set(
				0, 0, MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));

			child.Measure(widthMeasureSpec, heightMeasureSpec);

			SetMeasuredDimension(child.MeasuredWidth, child.MeasuredHeight);
		}

		public override void RequestLayout()
		{
			// Redraw shadow (if exists)
			_invalidateShadow = true;

			base.RequestLayout();
		}

		protected override void DispatchDraw(Canvas canvas)
		{
			// If is not shadowed, skip
			if (Shadow?.Paint != null)
			{
				DrawShadow(canvas);
			}
			else
			{
				if (_shadowBitmap != null)
				{
					ClearShadowResources();
				}
			}

			// Clip the child view
			if (Clip != null)
				ClipChild(canvas);

			// Draw child`s
			base.DispatchDraw(canvas);
		}

		public override bool DispatchTouchEvent(MotionEvent e)
		{
			if (InputTransparent)
			{
				return false;
			}

			return base.DispatchTouchEvent(e);
		}

		partial void ClipChanged()
		{
			_invalidateClip = _invalidateShadow = true;
			PostInvalidate();
		}

		partial void ShadowChanged()
		{
			_invalidateShadow = true;
			PostInvalidate();
		}

		partial void BorderChanged()
		{
			if (Border == null)
			{
				if (_borderView != null)
					this.RemoveView(_borderView);
				_borderView = null;
				return;
			}

			if (_borderView == null)
			{
				this.AddView(_borderView = new AView(Context));
			}
			_borderView.UpdateBorderStroke(Border);
		}

		void ClipChild(Canvas canvas)
		{
			var bounds = new Graphics.RectF(0, 0, canvas.Width, canvas.Height);

			if (_invalidateClip || _lastPathSize != bounds.Size || _currentPath == null)
			{
				_invalidateClip = false;

				var path = Clip.PathForBounds(bounds);
				_currentPath = path?.AsAndroidPath();
				_lastPathSize = bounds.Size;
			}

			if (_currentPath != null)
				canvas.ClipPath(_currentPath);
		}

		void DrawShadow(Canvas canvas)
		{
			if (_shadowCanvas == null)
				_shadowCanvas = new Canvas();

			if (_shadowPaint == null)
				_shadowPaint = new Android.Graphics.Paint
				{
					AntiAlias = true,
					Dither = true,
					FilterBitmap = true
				};

			Graphics.Color solidColor = null;

			// If need to redraw shadow
			if (_invalidateShadow)
			{
				// If bounds is zero
				if (_viewBounds.Width() != 0 && _viewBounds.Height() != 0)
				{
					var bitmapHeight = _viewBounds.Height() + MaximumRadius;
					var bitmapWidth = _viewBounds.Width() + MaximumRadius;

					// Reset bitmap to bounds
					_shadowBitmap = Bitmap.CreateBitmap(
						bitmapWidth, bitmapHeight, Bitmap.Config.Argb8888
					);

					// Reset Canvas
					_shadowCanvas.SetBitmap(_shadowBitmap);

					_invalidateShadow = false;

					// Create the local copy of all content to draw bitmap as a
					// bottom layer of natural canvas.
					base.DispatchDraw(_shadowCanvas);

					// Get the alpha bounds of bitmap
					Bitmap extractAlpha = _shadowBitmap.ExtractAlpha();

					// Clear past content content to draw shadow
					_shadowCanvas.DrawColor(Android.Graphics.Color.Black, PorterDuff.Mode.Clear);

					var shadowOpacity = (float)Shadow.Opacity;

					if (Shadow.Paint is LinearGradientPaint linearGradientPaint)
					{
						var linearGradientShaderFactory = PaintExtensions.GetLinearGradientShaderFactory(linearGradientPaint);
						_shadowPaint.SetShader(linearGradientShaderFactory.Resize(bitmapWidth, bitmapHeight));
					}
					if (Shadow.Paint is RadialGradientPaint radialGradientPaint)
					{
						var radialGradientShaderFactory = PaintExtensions.GetRadialGradientShaderFactory(radialGradientPaint);
						_shadowPaint.SetShader(radialGradientShaderFactory.Resize(bitmapWidth, bitmapHeight));
					}
					if (Shadow.Paint is SolidPaint solidPaint && OperatingSystem.IsAndroidVersionAtLeast(29))
					{
						solidColor = solidPaint.ToColor();
						_shadowPaint.Color = solidColor.WithAlpha(shadowOpacity).ToPlatform();
					}

					// Apply the shadow radius 
					var radius = Shadow.Radius;

					if (radius <= 0)
						radius = 0.01f;

					if (radius > 100)
						radius = MaximumRadius;

					_shadowPaint.SetMaskFilter(new BlurMaskFilter(radius, BlurMaskFilter.Blur.Normal));

					float shadowOffsetX = (float)Shadow.Offset.X;
					float shadowOffsetY = (float)Shadow.Offset.Y;

					if (Clip == null)
					{
						_shadowCanvas.DrawBitmap(extractAlpha, shadowOffsetX, shadowOffsetY, _shadowPaint);
					}
					else
					{
						var bounds = new Graphics.RectF(0, 0, canvas.Width, canvas.Height);
						var path = Clip.PathForBounds(bounds)?.AsAndroidPath();

						path.Offset(shadowOffsetX, shadowOffsetY);

						_shadowCanvas.DrawPath(path, _shadowPaint);
					}

					// Recycle and clear extracted alpha
					extractAlpha.Recycle();
				}
				else
				{
					// Create placeholder bitmap when size is zero and wait until new size coming up
					_shadowBitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Rgb565!);
				}
			}

			// Reset alpha to draw child with full alpha
			if (solidColor != null && OperatingSystem.IsAndroidVersionAtLeast(29))
				_shadowPaint.Color = solidColor.ToPlatform();

			// Draw shadow bitmap
			if (_shadowCanvas != null && _shadowBitmap != null && !_shadowBitmap.IsRecycled)
				canvas.DrawBitmap(_shadowBitmap, 0.0F, 0.0F, _shadowPaint);
		}

		void ClearShadowResources()
		{
			_shadowCanvas?.Dispose();
			_shadowPaint?.Dispose();
			_shadowBitmap?.Dispose();
			_shadowCanvas = null;
			_shadowPaint = null;
			_shadowBitmap = null;
		}
	}
}