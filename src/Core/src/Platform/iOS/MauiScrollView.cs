using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, ICrossPlatformLayoutBacking, IPlatformMeasureInvalidationController
	{
		bool _invalidateParentWhenMovedToWindow;
		double _lastMeasureHeight;
		double _lastMeasureWidth;
		double _lastArrangeHeight;
		double _lastArrangeWidth;

		UIUserInterfaceLayoutDirection? _previousEffectiveUserInterfaceLayoutDirection;

		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		ICrossPlatformLayout? ICrossPlatformLayoutBacking.CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		internal ICrossPlatformLayout? CrossPlatformLayout => ((ICrossPlatformLayoutBacking)this).CrossPlatformLayout;

		public override void LayoutSubviews()
		{
			// LayoutSubviews is invoked while scrolling, so we need to arrange the content only when it's necessary.
			// This could be done via `override ScrollViewHandler.PlatformArrange` but that wouldn't cover the case
			// when the ScrollView is attached to a non-MauiView parent (i.e. DeviceTests).
			var bounds = Bounds;
			var widthConstraint = (double)bounds.Width;
			var heightConstraint = (double)bounds.Height;
			var frameChanged = _lastArrangeWidth != widthConstraint || _lastArrangeHeight != heightConstraint;

			// If the frame changed, we need to arrange (and potentially measure) the content again
			if (frameChanged && CrossPlatformLayout is { } crossPlatformLayout)
			{
				_lastArrangeWidth = widthConstraint;
				_lastArrangeHeight = heightConstraint;

				if (!IsMeasureValid(widthConstraint, heightConstraint))
				{
					crossPlatformLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);
					CacheMeasureConstraints(widthConstraint, heightConstraint);
				}

				// Account for safe area adjustments automatically added by iOS
				var crossPlatformBounds = AdjustedContentInset.InsetRect(bounds).Size.ToSize();
				var crossPlatformContentSize = crossPlatformLayout.CrossPlatformArrange(new Rect(new Point(), crossPlatformBounds));
				var contentSize = crossPlatformContentSize.ToCGSize();

				// For Right-To-Left (RTL) layouts, we need to adjust the content arrangement and offset
				// to ensure the content is correctly aligned and scrolled. This involves a second layout
				// arrangement with an adjusted starting point and recalculating the content offset.
				if (_previousEffectiveUserInterfaceLayoutDirection != EffectiveUserInterfaceLayoutDirection)
				{
					// In mac platform, Scrollbar is not updated based on FlowDirection, so resetting the scroll indicators
					// It's a native limitation; to maintain platform consistency, a hack fix is applied to show the Scrollbar based on the FlowDirection.
					if (OperatingSystem.IsMacCatalyst() && _previousEffectiveUserInterfaceLayoutDirection is not null)
					{
						bool showsVertical = ShowsVerticalScrollIndicator;
						bool showsHorizontal = ShowsHorizontalScrollIndicator;

						ShowsVerticalScrollIndicator = false;
						ShowsHorizontalScrollIndicator = false;

						ShowsVerticalScrollIndicator = showsVertical;
						ShowsHorizontalScrollIndicator = showsHorizontal;
					}

					if (EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
					{
						var horizontalOffset = contentSize.Width - crossPlatformBounds.Width;
						crossPlatformLayout.CrossPlatformArrange(new Rect(new Point(-horizontalOffset, 0), crossPlatformBounds));
						ContentOffset = new CGPoint(horizontalOffset, 0);
					}
					else if (_previousEffectiveUserInterfaceLayoutDirection is not null)
					{
						ContentOffset = new CGPoint(0, ContentOffset.Y);
					}
				}

				// When switching between LTR and RTL, we need to re-arrange and offset content exactly once
				// to avoid cumulative shifts or incorrect offsets on subsequent layouts.
				_previousEffectiveUserInterfaceLayoutDirection = EffectiveUserInterfaceLayoutDirection;

				// When the content size changes, we need to adjust the scrollable area size so that the content can fit in it.
				if (ContentSize != contentSize)
				{
					ContentSize = contentSize;

					// Invalidation stops at `UIScrollViews` for performance reasons,
					// but when the content size changes, we need to invalidate the ancestors
					// in case the ScrollView is configured to grow/shrink with its content.
					this.InvalidateAncestorsMeasures();
				}
			}

			base.LayoutSubviews();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (CrossPlatformLayout is not { } crossPlatformLayout)
			{
				return new CGSize();
			}

			var widthConstraint = (double)size.Width;
			var heightConstraint = (double)size.Height;

			var contentSize = crossPlatformLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);
			CacheMeasureConstraints(widthConstraint, heightConstraint);

			return contentSize;
		}

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			SetNeedsLayout();
			InvalidateConstraintsCache();

			return !isPropagating;
		}

		bool IsMeasureValid(double widthConstraint, double heightConstraint)
		{
			// Check the last constraints this View was measured with; if they're the same,
			// then the current measure info is already correct, and we don't need to repeat it
			return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
		}

		void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
			_lastArrangeWidth = double.NaN;
			_lastArrangeHeight = double.NaN;
		}

		void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
		}

		// overriding this method so it does not automatically scroll large UITextFields
		// while the KeyboardAutoManagerScroll is scrolling.
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
			if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
				base.ScrollRectToVisible(rect, animated);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;

		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}
	}
}