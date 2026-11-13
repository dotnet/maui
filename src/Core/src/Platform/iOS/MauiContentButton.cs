using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal sealed class MauiContentButton : UIButton, ICrossPlatformLayoutBacking, IPlatformMeasureInvalidationController, IUIViewLifeCycleEvents
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The owned child does not reference this button; proven safe in MemoryTests.HandlerDoesNotLeak.")]
		readonly ContentContainer _contentContainer;

		bool _nativeDefaultsCaptured;
		UIColor? _defaultBackgroundColor;
		CGColor? _defaultBorderColor;
		nfloat _defaultBorderWidth;
		nfloat _defaultCornerRadius;

		bool _invalidateParentWhenMovedToWindow;
		WeakReference<UIView>? _contentView;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayout;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;

		internal MauiContentButton(IContentButton virtualView)
		{
			ClipsToBounds = true;
			IsAccessibilityElement = true;

			// Disabling interaction and accessibility on this owned container keeps the
			// presentation tree inert without changing state that belongs to the content views.
			_contentContainer = new ContentContainer
			{
				UserInteractionEnabled = false,
				IsAccessibilityElement = false,
				AccessibilityElementsHidden = true,
			};
			AddSubview(_contentContainer);
			SetVirtualView(virtualView);
		}

		ICrossPlatformLayout? ICrossPlatformLayoutBacking.CrossPlatformLayout
		{
			get => _crossPlatformLayout is not null && _crossPlatformLayout.TryGetTarget(out var layout) ? layout : null;
			set
			{
				_crossPlatformLayout = value is null ? null : new(value);
				_contentContainer.View = value as IView;
				_contentContainer.CrossPlatformLayout = value;
				_contentContainer.InvalidateContentMeasure();
				SetNeedsLayout();
			}
		}

		IContentButton? VirtualView => ((ICrossPlatformLayoutBacking)this).CrossPlatformLayout as IContentButton;

		internal UIView ContentContainerView => _contentContainer;

		event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		internal void SetVirtualView(IContentButton? virtualView)
		{
			((ICrossPlatformLayoutBacking)this).CrossPlatformLayout = virtualView;
		}

		internal void SetContent(UIView? content)
		{
			if (_contentView is not null &&
				_contentView.TryGetTarget(out var previousContent) &&
				ReferenceEquals(previousContent.Superview, _contentContainer))
			{
				previousContent.RemoveFromSuperview();
			}

			_contentView = null;

			if (content is not null)
			{
				content.RemoveFromSuperview();
				_contentContainer.AddSubview(content);
				_contentView = new(content);
			}

			this.InvalidateMeasure();
		}

		// Handler subclasses can customize the view returned by CreatePlatformView, so
		// defaults are captured on first use instead of during construction.
		internal void CaptureNativeDefaults()
		{
			if (_nativeDefaultsCaptured)
				return;

			_nativeDefaultsCaptured = true;
			_defaultBackgroundColor = BackgroundColor;
			_defaultBorderColor = Layer.BorderColor;
			_defaultBorderWidth = Layer.BorderWidth;
			_defaultCornerRadius = Layer.CornerRadius;
		}

		internal void UpdateBackground(IContentButton button)
		{
			CaptureNativeDefaults();
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
			CaptureNativeDefaults();
			Layer.BorderColor = button.StrokeColor?.ToCGColor() ?? _defaultBorderColor;
			UpdateBackground(button);
		}

		internal void UpdateStrokeThickness(IContentButton button)
		{
			CaptureNativeDefaults();
			Layer.BorderWidth = button.StrokeThickness >= 0 ? (nfloat)button.StrokeThickness : _defaultBorderWidth;
			UpdateBackground(button);
			this.InvalidateMeasure();
		}

		internal void UpdateCornerRadius(IContentButton button)
		{
			CaptureNativeDefaults();
			Layer.CornerRadius = button.CornerRadius >= 0 ? button.CornerRadius : _defaultCornerRadius;
			UpdateBackground(button);
			SetNeedsLayout();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (VirtualView is null)
				return base.SizeThatFits(size);

			return _contentContainer.SizeThatFits(size);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var bounds = new CGRect(CGPoint.Empty, Bounds.Size);

			if (VirtualView is not null && !this.IsFinalMeasureHandledBySuperView())
				_contentContainer.SizeThatFits(bounds.Size);

			_contentContainer.Frame = bounds;
			_contentContainer.LayoutIfNeeded();
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

			_movedToWindow?.Invoke(this, EventArgs.Empty);

			if (_invalidateParentWhenMovedToWindow && Window is not null)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			_contentContainer.InvalidateContentMeasure();
			SetNeedsLayout();
			return true;
		}

		// Reuses MauiView's safe-area, keyboard and measure-cache behavior for the presented content.
		sealed class ContentContainer : MauiView
		{
			internal void InvalidateContentMeasure()
			{
				InvalidateConstraintsCache();
				SetNeedsLayout();
			}
		}
	}
}
