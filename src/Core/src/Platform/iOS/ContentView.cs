using System;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class ContentView : MauiView
	{
		WeakReference<IBorderStroke>? _clip;
		CAShapeLayer? _childMaskLayer;
		internal event EventHandler? LayoutSubviewsChanged;

		public ContentView()
		{
			Layer.CornerCurve = CACornerCurve.Continuous;
		}

		public override CGSize SizeThatFits(CGSize size)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var crossPlatformMeasure = CrossPlatformMeasure;
#pragma warning restore CS0618 // Type or member is obsolete

			if (crossPlatformMeasure is null && View is IContentView view)
			{
				crossPlatformMeasure = view.CrossPlatformMeasure;
			}

			if (crossPlatformMeasure is null)
			{
				return base.SizeThatFits(size);
			}

			var widthConstraint = size.Width;
			var heightConstraint = size.Height;

			var crossPlatformSize = crossPlatformMeasure(widthConstraint, heightConstraint);

			CacheMeasureConstraints(widthConstraint, heightConstraint);

			return crossPlatformSize.ToCGSize();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

#pragma warning disable CS0618 // Type or member is obsolete
			var crossPlatformMeasure = CrossPlatformMeasure;
			var crossPlatformArrange = CrossPlatformArrange;
#pragma warning restore CS0618 // Type or member is obsolete

			var view = View as IContentView;
			if (crossPlatformMeasure is null && view is not null)
			{
				crossPlatformMeasure = view.CrossPlatformMeasure;
			}
			if (crossPlatformArrange is null && view is not null)
			{
				crossPlatformArrange = view.CrossPlatformArrange;
			}

			if (crossPlatformMeasure is null || crossPlatformArrange is null)
			{
				return;
			}

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();
			var widthConstraint = bounds.Width;
			var heightConstraint = bounds.Height;

			// If the SuperView is a MauiView (backing a cross-platform ContentView or Layout), then measurement
			// has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews. But we
			// _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
			// the window); there's no guarantee that SizeThatFits has been called in that case.

			if (!IsMeasureValid(widthConstraint, heightConstraint) && Superview is not MauiView)
			{
				crossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasureConstraints(widthConstraint, heightConstraint);
			}

			crossPlatformArrange(bounds);

			if (ChildMaskLayer != null)
				ChildMaskLayer.Frame = bounds;

			SetClip();

			LayoutSubviewsChanged?.Invoke(this, EventArgs.Empty);
		}

		public override void SetNeedsLayout()
		{
			InvalidateConstraintsCache();
			base.SetNeedsLayout();
			Superview?.SetNeedsLayout();
		}

		[Obsolete("Use View instead. This member causes memory leaks.")]
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		[Obsolete("Use View instead. This member causes memory leaks.")]
		internal Func<Rect, Size>? CrossPlatformArrange { get; set; }

		internal IBorderStroke? Clip
		{
			get
			{
				if (_clip?.TryGetTarget(out IBorderStroke? target) == true)
					return target;

				return null;
			}
			set
			{
				_clip = null;

				if (value != null)
					_clip = new WeakReference<IBorderStroke>(value);

				SetClip();
			}
		}

		internal CAShapeLayer? ChildMaskLayer
		{
			get => _childMaskLayer;
			set
			{
				var layer = GetChildLayer();

				if (layer != null && _childMaskLayer != null)
					layer.Mask = null;

				_childMaskLayer = value;

				if (layer != null)
					layer.Mask = value;
			}
		}

		CALayer? GetChildLayer()
		{
			if (Subviews.Length == 0)
				return null;

			var child = Subviews[0];

			if (child.Layer is null)
				return null;

			return child.Layer;
		}

		void SetClip()
		{
			if (Subviews.Length == 0)
				return;

			var maskLayer = ChildMaskLayer;

			if (maskLayer is null && Clip is null)
				return;

			maskLayer ??= ChildMaskLayer = new CAShapeLayer();

			var frame = Frame;

			if (frame == CGRect.Empty)
				return;

			var strokeThickness = (float)(Clip?.StrokeThickness ?? 0);

			// In the MauiCALayer class, the Stroke is inner and we are clipping the outer, for that reason,
			// we use the double to get the correct value. Here, again, we use the double to get the correct clip shape size values.
			var strokeWidth = 2 * strokeThickness;

			var bounds = new RectF(0, 0, (float)frame.Width - strokeWidth, (float)frame.Height - strokeWidth);

			IShape? clipShape = Clip?.Shape;
			PathF? path;

			if (clipShape is IRoundRectangle roundRectangle)
				path = roundRectangle.InnerPathForBounds(bounds, strokeThickness);
			else
				path = clipShape?.PathForBounds(bounds);

			var nativePath = path?.AsCGPath();

			maskLayer.Path = nativePath;
		}
	}
}