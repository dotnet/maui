#nullable disable
using Android.Content;
using Android.Graphics;
using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;
using APath = Android.Graphics.Path;

namespace Microsoft.Maui
{
	public partial class WrapperView : ViewGroup
	{
		const int MaximumRadius = 100;

		readonly Rect _viewBounds;

		APath _currentPath;
		SizeF _lastPathSize;

		Bitmap _shadowBitmap;
		Canvas _shadowCanvas;
		Android.Graphics.Paint _shadowPaint;
		bool _invalidateShadow;

		public WrapperView(Context context)
			: base(context)
		{
			_viewBounds = new Rect();

			SetClipChildren(false);
			SetWillNotDraw(true);
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();

			if (_shadowBitmap != null)
			{
				_shadowBitmap.Recycle();
				_shadowBitmap = null;
			}
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (ChildCount == 0 || GetChildAt(0) is not View child)
				return;

			var widthMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(right - left);
			var heightMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(bottom - top);

			child.Measure(widthMeasureSpec, heightMeasureSpec);
			child.Layout(0, 0, child.MeasuredWidth, child.MeasuredHeight);
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
			if (Shadow != null && !Shadow.Value.IsEmpty)
				DrawShadow(canvas);
			else
			{
				if(_shadowBitmap != null)
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

		partial void ClipChanged()
		{
			_invalidateShadow = true;
			PostInvalidate();
		}

		partial void ShadowChanged()
		{
			_invalidateShadow = true;
			PostInvalidate();
		}

		void ClipChild(Canvas canvas)
		{
			var bounds = new RectangleF(0, 0, canvas.Width, canvas.Height);

			if (_lastPathSize != bounds.Size || _currentPath == null)
			{
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

			Graphics.Color shadowColor = Shadow.Value.Color;
			
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

					var shadowOpacity = Shadow.Value.Opacity;

					// Draw extracted alpha bounds of our local canvas
					_shadowPaint.Color = shadowColor.WithAlpha(shadowOpacity).ToNative();

					// Apply the shadow radius 
					float radius = Shadow.Value.Radius;

					if (radius <= 0)
						radius = 0.01f;

					if (radius > 100)
						radius = MaximumRadius;

					_shadowPaint.SetMaskFilter(new BlurMaskFilter(radius, BlurMaskFilter.Blur.Normal));

					float shadowOffsetX = (float)Shadow.Value.Offset.Width;
					float shadowOffsetY = (float)Shadow.Value.Offset.Height;

					if (Clip == null)
						_shadowCanvas.DrawBitmap(extractAlpha, shadowOffsetX, shadowOffsetY, _shadowPaint);
					else
					{
						var bounds = new RectangleF(0, 0, canvas.Width, canvas.Height);
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
			_shadowPaint.Color = shadowColor.ToNative();

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