#nullable disable
using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using APath = Android.Graphics.Path;
using AView = Android.Views.View;
using Rectangle = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : PlatformWrapperView, IVisualTreeElementProvidable
	{
		APath _currentPath;
		SizeF _lastPathSize;
		bool _invalidateClip;

		AView _borderView;

		public bool InputTransparent { get; set; }

		public WrapperView(Context context)
			: base(context)
		{
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);

			_borderView?.BringToFront();

			if (ChildCount == 0 || GetChildAt(0) is not AView child)
				return;

			// Apply safe area adjustments for WrapperView since it handles visual effects
			var contentBounds = new Rectangle(0, 0, right - left, bottom - top);
			var adjustedBounds = AdjustForSafeArea(contentBounds);

			// Convert adjusted bounds to pixels and position the child
			var leftPx = (int)Context.ToPixels(adjustedBounds.X);
			var topPx = (int)Context.ToPixels(adjustedBounds.Y);
			var rightPx = (int)Context.ToPixels(adjustedBounds.Right);
			var bottomPx = (int)Context.ToPixels(adjustedBounds.Bottom);

			var widthMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(rightPx - leftPx);
			var heightMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(bottomPx - topPx);

			child.Measure(widthMeasureSpec, heightMeasureSpec);
			child.Layout(leftPx, topPx, rightPx, bottomPx);
			_borderView?.Layout(leftPx, topPx, rightPx, bottomPx);
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
			SetHasClip(Clip is not null);
		}

		partial void ShadowChanged()
		{
			if (Shadow?.Paint is { } shadowPaint)
			{
				var context = Context;
				var shadowOpacity = Shadow.Opacity;
				float radius = context.ToPixels(Shadow.Radius);
				float offsetX = context.ToPixels(Shadow.Offset.X);
				float offsetY = context.ToPixels(Shadow.Offset.Y);
				int paintType;
				int[] colors;
				float[] positions;
				float[] bounds;

				switch (shadowPaint)
				{
					case LinearGradientPaint linearGradientPaint:
						var linearGradientData = linearGradientPaint.GetGradientData(shadowOpacity);
						paintType = PlatformPaintType.Linear;
						colors = linearGradientData.Colors;
						positions = linearGradientData.Offsets;
						bounds = [linearGradientData.X1, linearGradientData.Y1, linearGradientData.X2, linearGradientData.Y2];
						break;
					case RadialGradientPaint radialGradientPaint:
						var radialGradientData = radialGradientPaint.GetGradientData(shadowOpacity);
						paintType = PlatformPaintType.Radial;
						colors = radialGradientData.Colors;
						positions = radialGradientData.Offsets;
						bounds = [radialGradientData.CenterX, radialGradientData.CenterY, radialGradientData.Radius];
						break;
					case SolidPaint solidPaint:
						paintType = PlatformPaintType.Solid;
						// If the alpha is set in the color value, the shadow transparency is applied based on that alpha. 
						// If the Opacity property is set directly, the shadow transparency is applied based on the Opacity. 
						// If both values are provided, the color alpha is combined with the Opacity to apply a unified transparency effect to the shadow, ensuring consistent behavior across platforms.
						colors = [solidPaint.Color.WithAlpha(solidPaint.Color.Alpha * shadowOpacity).ToPlatform().ToArgb()];
						positions = null;
						bounds = null;
						break;
					default:
						throw new NotSupportedException("Unsupported shadow paint type.");
				}

				UpdateShadow(paintType, radius, offsetX, offsetY, colors, positions, bounds);
			}
			else
			{
				UpdateShadow(PlatformPaintType.None, 0, 0, 0, null, null, null);
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

		Rectangle AdjustForSafeArea(Rectangle bounds)
		{
			// Only apply safe area adjustments if the child is a layout that cares about safe areas
			if (ChildCount > 0 && GetChildAt(0) is LayoutViewGroup layout && 
				layout.CrossPlatformLayout is ISafeAreaView sav && !sav.IgnoreSafeArea)
			{
				var insets = ViewCompat.GetRootWindowInsets(this);
				if (insets != null)
				{
					// Get system window insets (status bar, navigation bar, etc.)
					var systemInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
					var safeAreaInsets = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

					// Use the maximum of system insets and cutout insets for true safe area
					var left = Math.Max(systemInsets?.Left ?? 0, safeAreaInsets?.Left ?? 0);
					var top = Math.Max(systemInsets?.Top ?? 0, safeAreaInsets?.Top ?? 0);
					var right = Math.Max(systemInsets?.Right ?? 0, safeAreaInsets?.Right ?? 0);
					var bottom = Math.Max(systemInsets?.Bottom ?? 0, safeAreaInsets?.Bottom ?? 0);

					// Convert Android pixels to device-independent units
					var leftDip = Context?.FromPixels(left) ?? 0;
					var topDip = Context?.FromPixels(top) ?? 0;
					var rightDip = Context?.FromPixels(right) ?? 0;
					var bottomDip = Context?.FromPixels(bottom) ?? 0;

					// Apply safe area insets to bounds
					return new Rectangle(
						bounds.X + leftDip,
						bounds.Y + topDip,
						bounds.Width - leftDip - rightDip,
						bounds.Height - topDip - bottomDip);
				}
			}

			return bounds;
		}

		public override WindowInsets OnApplyWindowInsets(WindowInsets insets)
		{
			// Trigger layout update if we have a layout that cares about safe area
			if (ChildCount > 0 && GetChildAt(0) is LayoutViewGroup layout && 
				layout.CrossPlatformLayout is ISafeAreaView sav && !sav.IgnoreSafeArea)
			{
				RequestLayout();
			}

			return base.OnApplyWindowInsets(insets);
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

		IVisualTreeElement IVisualTreeElementProvidable.GetElement()
		{
			// Return the element from the wrapped layout if it exists
			if (ChildCount > 0 && GetChildAt(0) is IVisualTreeElementProvidable provider)
			{
				return provider.GetElement();
			}

			return null;
		}
	}
}
