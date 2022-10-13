#nullable disable
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

		readonly ShadowCache _cache;

		AView _borderView;

		public bool InputTransparent { get; set; }

		public WrapperView(Context context)
			: base(context)
		{
			_viewBounds = new Android.Graphics.Rect();
			_cache = ShadowCache.Instance;

			SetClipChildren(false);
			SetWillNotDraw(true);
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
			base.RequestLayout();

			InvalidateShadow();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_cache.Dispose();
			}
		}

		protected override void DispatchDraw(Canvas canvas)
		{
			// Draw the shadow (if exists)
			if (Shadow != null)
				DrawShadow(canvas);

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
			_invalidateClip = true;
			PostInvalidate();
		}

		partial void ShadowChanged()
		{
			InvalidateShadow();
			PostInvalidate();
		}

		partial void BorderChanged()
		{
			if (Border == null)
			{
				if (_borderView != null)
					RemoveView(_borderView);
				_borderView = null;
				return;
			}

			if (_borderView == null)
			{
				AddView(_borderView = new AView(Context));
			}

			_borderView.UpdateBorderStroke(Border);
		}

		void ClipChild(Canvas canvas)
		{
			var density = Context.GetDisplayDensity();
			var newSize = new SizeF(canvas.Width, canvas.Height);
			var bounds = new Graphics.RectF(Graphics.Point.Zero, newSize / density);

			if (_invalidateClip || _lastPathSize != newSize || _currentPath == null)
			{
				_invalidateClip = false;

				var path = Clip.PathForBounds(bounds);
				_currentPath = path?.AsAndroidPath(scaleX: density, scaleY: density);
				_lastPathSize = newSize;
			}

			if (_currentPath != null)
				canvas.ClipPath(_currentPath);
		}

		string GetShadowHash()
		{
			if (Shadow == null)
				return string.Empty;

			return Shadow.GetHashCode().ToString();
		}

		void DrawShadow(Canvas canvas)
		{
			if (_cache == null)
				return;

			if (GetChildAt(0) is AView child)
			{
				var shadow = _cache.Add(GetShadowHash(), CreateShadow);

				if (shadow == null || shadow.IsDisposed())
					return;

				float x = child.GetX() + (float)Shadow.Offset.X;
				float y = child.GetY() + (float)Shadow.Offset.Y;

				canvas.DrawBitmap(shadow, x, y, null);
			}
		}

		Bitmap CreateShadow()
		{
			if (_cache == null || Shadow == null)
				return null;

			var viewHeight = _viewBounds.Height();
			var viewWidth = _viewBounds.Width();

			if (GetChildAt(0) is AView child)
			{
				if (viewHeight == 0)
					viewHeight = child.MeasuredHeight;

				if (viewWidth == 0)
					viewWidth = child.MeasuredWidth;
			}

			if (viewHeight != 0 && viewWidth != 0)
			{
				var bitmapHeight = viewHeight + MaximumRadius;
				var bitmapWidth = viewWidth + MaximumRadius;

				// Create the Shadow bitmap
				var shadowBitmap = Bitmap.CreateBitmap(bitmapWidth, bitmapHeight, Bitmap.Config.Argb8888);

				using var shadowCanvas = new Canvas(shadowBitmap);
				using var shadowPaint = new Android.Graphics.Paint { AntiAlias = false, FilterBitmap = false };
				{
					// Reset Canvas
					shadowCanvas.SetBitmap(shadowBitmap);

					// Create the local copy of all content to draw bitmap as a
					// bottom layer of natural canvas.
					base.DispatchDraw(shadowCanvas);

					// Get the alpha bounds of bitmap
					Bitmap extractAlpha = shadowBitmap.ExtractAlpha();

					// Clear past content content to draw shadow
					shadowCanvas.DrawColor(Android.Graphics.Color.Black, PorterDuff.Mode.Clear);

					var shadowOpacity = (float)Shadow.Opacity;

					if (Shadow.Paint is LinearGradientPaint linearGradientPaint)
					{
						var linearGradientShaderFactory = PaintExtensions.GetLinearGradientShaderFactory(linearGradientPaint, shadowOpacity);
						shadowPaint.SetShader(linearGradientShaderFactory.Resize(bitmapWidth, bitmapHeight));
					}

					if (Shadow.Paint is RadialGradientPaint radialGradientPaint)
					{
						var radialGradientShaderFactory = PaintExtensions.GetRadialGradientShaderFactory(radialGradientPaint, shadowOpacity);
						shadowPaint.SetShader(radialGradientShaderFactory.Resize(bitmapWidth, bitmapHeight));
					}

					Graphics.Color solidColor = null;

					if (Shadow.Paint is SolidPaint solidPaint)
					{
						solidColor = solidPaint.ToColor();
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
						shadowPaint.Color = solidColor.WithAlpha(shadowOpacity).ToPlatform();
#pragma warning restore CA1416
					}

					// Apply the shadow radius 
					var radius = Shadow.Radius;

					if (radius <= 0)
						radius = 0.01f;

					if (radius > 100)
						radius = MaximumRadius;

					shadowPaint.SetMaskFilter(new BlurMaskFilter(radius, BlurMaskFilter.Blur.Normal));

					float shadowOffsetX = (float)Shadow.Offset.X;
					float shadowOffsetY = (float)Shadow.Offset.Y;

					if (Clip == null)
					{
						shadowCanvas.DrawBitmap(extractAlpha, shadowOffsetX, shadowOffsetY, shadowPaint);
					}
					else
					{
						var bounds = new Graphics.RectF(0, 0, viewWidth, viewHeight);
						var path = Clip.PathForBounds(bounds)?.AsAndroidPath();
						path.Offset(shadowOffsetX, shadowOffsetY);
						shadowCanvas.DrawPath(path, shadowPaint);
					}

					// Recycle and clear extracted alpha
					extractAlpha.Recycle();
				}

				return shadowBitmap;
			}
			else
			{
				return null;
			}
		}

		void InvalidateShadow()
		{
			if (_cache == null || Shadow == null)
				return;

			var shadowHash = GetShadowHash();
			_cache.Remove(shadowHash);
			_cache.Add(shadowHash, CreateShadow);
		}
	}
}