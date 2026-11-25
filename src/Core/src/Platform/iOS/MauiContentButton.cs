using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
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

		bool _isFocusedSetByUs;
		bool _isNativeFocused;
		bool _invalidateParentWhenMovedToWindow;
		WeakReference<UIView>? _contentView;
		int _contentRequest;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayout;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;

		internal MauiContentButton(IContentButton virtualView)
		{
			ClipsToBounds = true;
			IsAccessibilityElement = true;
			_contentContainer = AddContentContainer();
			SetVirtualView(virtualView);
		}

		// UIKit can retain this view after its managed peer is collected; Superview lookups
		// then recreate the peer from the native handle.
		MauiContentButton(NativeHandle handle)
			: base(handle)
		{
			ContentContainer? existing = null;
			foreach (var subview in Subviews)
			{
				if (subview is ContentContainer container)
				{
					existing = container;
					break;
				}
			}

			_contentContainer = existing ?? AddContentContainer();
		}

		ContentContainer AddContentContainer()
		{
			// Disabling interaction and accessibility on this owned container keeps the
			// presentation tree inert without changing state that belongs to the content views.
			var container = new ContentContainer
			{
				UserInteractionEnabled = false,
				IsAccessibilityElement = false,
				AccessibilityElementsHidden = true,
			};
			AddSubview(container);
			return container;
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

		internal MauiView ContentContainerView => _contentContainer;

		event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		internal void SetVirtualView(IContentButton? virtualView)
		{
			var previousView = VirtualView;
			var clearPreviousFocus = !ReferenceEquals(previousView, virtualView) && _isFocusedSetByUs;

			if (!ReferenceEquals(previousView, virtualView))
				_isFocusedSetByUs = false;

			((ICrossPlatformLayoutBacking)this).CrossPlatformLayout = virtualView;

			if (clearPreviousFocus && previousView is IView { IsFocused: true } previous)
				previous.IsFocused = false;

			if (_isNativeFocused && ReferenceEquals(VirtualView, virtualView) &&
				virtualView is IView { IsFocused: false } current)
			{
				_isFocusedSetByUs = true;
				current.IsFocused = true;
			}
		}

		public override void DidUpdateFocus(UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator)
		{
			base.DidUpdateFocus(context, coordinator);

			_isNativeFocused = context.NextFocusedView == this;
			if (_isNativeFocused)
			{
				if (VirtualView is IView { IsFocused: false } view)
				{
					_isFocusedSetByUs = true;
					view.IsFocused = true;
				}
			}
			else if (_isFocusedSetByUs)
			{
				_isFocusedSetByUs = false;
				if (VirtualView is IView { IsFocused: true } view)
					view.IsFocused = false;
			}
		}

		internal void SetContent(UIView? content)
		{
			// Moving views runs synchronous UIKit callbacks (window and superview changes) that can
			// map newer content or disconnect the handler before this call finishes. Stop as soon
			// as this request is superseded so stale content is never attached.
			var request = ++_contentRequest;

			if (_contentView is not null &&
				_contentView.TryGetTarget(out var previousContent) &&
				ReferenceEquals(previousContent.Superview, _contentContainer))
			{
				previousContent.RemoveFromSuperview();
				if (request != _contentRequest)
					return;
			}

			// Track the content before adding it so a nested request can remove it again.
			_contentView = content is null ? null : new(content);

			if (content is not null)
			{
				content.RemoveFromSuperview();
				if (request != _contentRequest)
					return;

				_contentContainer.AddSubview(content);
				if (request != _contentRequest)
					return;
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

			if (button.Background.IsNullOrEmpty())
			{
				this.RemoveBackgroundLayer();
				BackgroundColor = _defaultBackgroundColor;
				return;
			}

			// The shared helper removes the previous background layer itself.
			ViewExtensions.UpdateBackground(this, button.Background, button);
		}

		internal void UpdateStrokeColor(IContentButton button)
		{
			CaptureNativeDefaults();
			Layer.BorderColor = button.StrokeColor?.ToCGColor() ?? _defaultBorderColor;
		}

		internal void UpdateStrokeThickness(IContentButton button)
		{
			CaptureNativeDefaults();
			Layer.BorderWidth = button.StrokeThickness >= 0 ? (nfloat)button.StrokeThickness : _defaultBorderWidth;
			this.InvalidateMeasure();
		}

		internal void UpdateCornerRadius(IContentButton button)
		{
			CaptureNativeDefaults();
			Layer.CornerRadius = button.CornerRadius >= 0 ? button.CornerRadius : _defaultCornerRadius;
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
			public ContentContainer()
			{
			}

			ContentContainer(NativeHandle handle)
				: base(handle)
			{
			}

			internal override void ArrangeAdjustedBounds(CGRect bounds)
			{
				if (EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
				{
					// Child arrangement mirrors within the full container; keep safe-area edges physical.
					bounds.X = 2 * Bounds.X + Bounds.Width - bounds.X - bounds.Width;
				}

				base.ArrangeAdjustedBounds(bounds);
			}

			internal void InvalidateContentMeasure()
			{
				InvalidateConstraintsCache();
				SetNeedsLayout();
			}
		}
	}
}
