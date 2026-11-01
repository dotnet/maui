using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal sealed class MauiContentButton : UIButton, ICrossPlatformLayoutBacking, IPlatformMeasureInvalidationController
	{
		readonly UIColor? _defaultBackgroundColor;
		readonly CGColor? _defaultBorderColor;
		readonly nfloat _defaultCornerRadius;

		bool _invalidateParentWhenMovedToWindow;
		Size? _lastMeasuredSize;
		double _lastMeasureHeight = double.NaN;
		double _lastMeasureWidth = double.NaN;
		WeakReference<UIView>? _contentView;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayout;

		internal MauiContentButton(IContentButton virtualView)
		{
			_defaultBackgroundColor = BackgroundColor;
			_defaultBorderColor = Layer.BorderColor;
			_defaultCornerRadius = Layer.CornerRadius;
			ClipsToBounds = true;
			IsAccessibilityElement = true;
			SetVirtualView(virtualView);
		}

		ICrossPlatformLayout? ICrossPlatformLayoutBacking.CrossPlatformLayout
		{
			get => _crossPlatformLayout is not null && _crossPlatformLayout.TryGetTarget(out var layout) ? layout : null;
			set
			{
				_crossPlatformLayout = value is null ? null : new(value);
				InvalidateConstraintsCache();
			}
		}

		IContentButton? VirtualView => ((ICrossPlatformLayoutBacking)this).CrossPlatformLayout as IContentButton;

		internal void SetVirtualView(IContentButton? virtualView)
		{
			((ICrossPlatformLayoutBacking)this).CrossPlatformLayout = virtualView;
		}

		internal void SetContent(UIView? content)
		{
			if (_contentView is not null && _contentView.TryGetTarget(out var previousContent))
				previousContent.RemoveFromSuperview();

			_contentView = null;

			if (content is not null)
			{
				content.RemoveFromSuperview();
				content.UserInteractionEnabled = false;
				content.IsAccessibilityElement = false;
				content.AccessibilityElementsHidden = true;
				AddSubview(content);
				_contentView = new(content);
			}

			this.InvalidateMeasure();
		}

		internal void UpdateBackground(IContentButton button)
		{
			this.RemoveBackgroundLayer();

			if (button.Background.IsNullOrEmpty())
			{
				BackgroundColor = _defaultBackgroundColor;
				return;
			}

			ViewExtensions.UpdateBackground(this, button.Background, button);
		}

		internal void UpdateStrokeColor(IContentButton button)
		{
			Layer.BorderColor = button.StrokeColor?.ToCGColor() ?? _defaultBorderColor;
			UpdateBackground(button);
		}

		internal void UpdateStrokeThickness(IContentButton button)
		{
			Layer.BorderWidth = (nfloat)Math.Max(0, button.StrokeThickness);
			UpdateBackground(button);
			this.InvalidateMeasure();
		}

		internal void UpdateCornerRadius(IContentButton button)
		{
			Layer.CornerRadius = button.CornerRadius >= 0 ? button.CornerRadius : _defaultCornerRadius;
			UpdateBackground(button);
			SetNeedsLayout();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (VirtualView is not IContentButton button)
				return base.SizeThatFits(size);

			var widthConstraint = (double)size.Width;
			var heightConstraint = (double)size.Height;

			if (widthConstraint == _lastMeasureWidth &&
				heightConstraint == _lastMeasureHeight &&
				_lastMeasuredSize is { } measuredSize)
			{
				return measuredSize.ToCGSize();
			}

			measuredSize = ((IContentView)button).CrossPlatformMeasure(widthConstraint, heightConstraint);
			CacheMeasure(widthConstraint, heightConstraint, measuredSize);
			return measuredSize.ToCGSize();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (VirtualView is not IContentButton button)
				return;

			var bounds = Bounds.ToRectangle();
			var widthConstraint = bounds.Width;
			var heightConstraint = bounds.Height;

			if (_lastMeasuredSize is null ||
				(widthConstraint != _lastMeasureWidth || heightConstraint != _lastMeasureHeight) &&
				!this.IsFinalMeasureHandledBySuperView())
			{
				var measuredSize = ((IContentView)button).CrossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasure(widthConstraint, heightConstraint, measuredSize);
			}

			((IContentView)button).CrossPlatformArrange(bounds);
		}

		public override bool AccessibilityActivate()
		{
			if (!Enabled)
				return false;

			SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
			return true;
		}

		public override UIView? HitTest(CGPoint point, UIEvent? uievent) =>
			base.HitTest(point, uievent) is null ? null : this;

		public override void TouchesEnded(NSSet touches, UIEvent? evt)
		{
			// UIKit permits a release slightly outside a button. ContentButton requires
			// the release to be inside its bounds, including when hosted in a scroll view.
			if (touches.AnyObject is UITouch touch && !PointInside(touch.LocationInView(this), evt))
				base.TouchesCancelled(touches, evt);
			else
				base.TouchesEnded(touches, evt);
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();

			if (Window is null)
				SendActionForControlEvents(UIControlEvent.TouchCancel);

			if (_invalidateParentWhenMovedToWindow && Window is not null)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}

		void CacheMeasure(double widthConstraint, double heightConstraint, Size measuredSize)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
			_lastMeasuredSize = measuredSize;
		}

		void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
			_lastMeasuredSize = null;
		}

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			InvalidateConstraintsCache();
			SetNeedsLayout();
			return true;
		}
	}
}
