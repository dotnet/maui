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
		APath _currentPath;
		SizeF _lastPathSize;
		bool _invalidateClip;

		AView _borderView;

		public bool InputTransparent { get; set; }

		internal AView WrappedView => ChildCount > 0 ? GetChildAt(0) : null;

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

			var widthMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(right - left);
			var heightMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(bottom - top);

			child.Measure(widthMeasureSpec, heightMeasureSpec);
			child.Layout(0, 0, child.MeasuredWidth, child.MeasuredHeight);
			_borderView?.Layout(0, 0, child.MeasuredWidth, child.MeasuredHeight);
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
