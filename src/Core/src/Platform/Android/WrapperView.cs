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
	public partial class WrapperView : PlatformWrapperView
	{
		const int MaximumRadius = 100;

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

		public override void RequestLayout()
		{
			// Redraw shadow (if exists)
			_invalidateShadow = true;

			base.RequestLayout();
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
			SetHasClip(Clip is not null);
		}

		partial void ShadowChanged()
		{
			_invalidateShadow = true;

			bool hasShadow = Shadow?.Paint is not null;
			SetHasShadow(hasShadow);
			if (!hasShadow && _shadowBitmap is not null)
			{
				ClearShadowResources();
			}
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

		protected override APath GetClipPath(int width, int height)
		{
			var density = Context.GetDisplayDensity();
			var newSize = new SizeF(width, height);
			var bounds = new Graphics.RectF(Graphics.Point.Zero, newSize / density);

			if (_invalidateClip || _lastPathSize != newSize || _currentPath == null)
			{
				_invalidateClip = false;

				var path = Clip.PathForBounds(bounds);
				_currentPath = path?.AsAndroidPath(scaleX: density, scaleY: density);
				_lastPathSize = newSize;
			}

			return _currentPath;
		}

		protected override void DrawShadow(Canvas canvas, int viewWidth, int viewHeight)
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
				if (viewHeight != 0 && viewWidth != 0)
				{
					var bitmapHeight = viewHeight + MaximumRadius;
					var bitmapWidth = viewWidth + MaximumRadius;

					// Reset bitmap to bounds
					_shadowBitmap = Bitmap.CreateBitmap(
						bitmapWidth, bitmapHeight, Bitmap.Config.Argb8888
					);

					// Reset Canvas
					_shadowCanvas.SetBitmap(_shadowBitmap);

					_invalidateShadow = false;

					// Create the local copy of all content to draw bitmap as a
					// bottom layer of natural canvas.
					ViewGroupDispatchDraw(_shadowCanvas);

					// Get the alpha bounds of bitmap
					Bitmap extractAlpha = _shadowBitmap.ExtractAlpha();

					// Clear past content content to draw shadow
					_shadowCanvas.DrawColor(Android.Graphics.Color.Black, PorterDuff.Mode.Clear);

					var shadowOpacity = (float)Shadow.Opacity;

					if (Shadow.Paint is LinearGradientPaint linearGradientPaint)
					{
						var linearGradientShaderFactory = PaintExtensions.GetGradientShaderFactory(linearGradientPaint, shadowOpacity);
						_shadowPaint.SetShader(linearGradientShaderFactory.Resize(bitmapWidth, bitmapHeight));
					}
					if (Shadow.Paint is RadialGradientPaint radialGradientPaint)
					{
						var radialGradientShaderFactory = PaintExtensions.GetGradientShaderFactory(radialGradientPaint, shadowOpacity);
						_shadowPaint.SetShader(radialGradientShaderFactory.Resize(bitmapWidth, bitmapHeight));
					}
					if (Shadow.Paint is SolidPaint solidPaint)
					{
						solidColor = solidPaint.ToColor();
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
						_shadowPaint.Color = solidColor.WithAlpha(shadowOpacity).ToPlatform();
#pragma warning restore CA1416
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
						var density = Context.GetDisplayDensity();
						var path = Clip.PathForBounds(bounds)?.AsAndroidPath(scaleX: density, scaleY: density);

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
			if (solidColor != null)
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
				_shadowPaint.Color = solidColor.ToPlatform();
#pragma warning restore CA1416

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

		public override ViewStates Visibility
		{
			get => base.Visibility;
			set
			{
				base.Visibility = value;

				if (value != ViewStates.Visible)
				{
					return;
				}

				for (int n = 0; n < this.ChildCount; n++)
				{
					var child = GetChildAt(n);
					child.Visibility = ViewStates.Visible;
				}
			}
		}

		internal static void SetupContainer(AView platformView, Context context, AView containerView, Action<AView> setWrapperView)
		{
			if (context == null || platformView == null || containerView != null)
				return;

			var oldParent = (ViewGroup)platformView.Parent;

			var oldIndex = oldParent?.IndexOfChild(platformView);
			oldParent?.RemoveView(platformView);

			containerView ??= new WrapperView(context);
			setWrapperView.Invoke(containerView);

			((ViewGroup)containerView).AddView(platformView);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.AddView(containerView, idx);
			else
				oldParent?.AddView(containerView);
		}

		internal static void RemoveContainer(AView platformView, Context context, AView containerView, Action clearWrapperView)
		{
			if (context == null || platformView == null || containerView == null || platformView.Parent != containerView)
			{
				CleanupContainerView(containerView, clearWrapperView);
				return;
			}

			var oldParent = (ViewGroup)containerView.Parent;

			var oldIndex = oldParent?.IndexOfChild(containerView);
			oldParent?.RemoveView(containerView);

			CleanupContainerView(containerView, clearWrapperView);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.AddView(platformView, idx);
			else
				oldParent?.AddView(platformView);

			void CleanupContainerView(AView containerView, Action clearWrapperView)
			{
				if (containerView is ViewGroup vg)
					vg.RemoveAllViews();

				clearWrapperView.Invoke();
			}
		}
	}
}