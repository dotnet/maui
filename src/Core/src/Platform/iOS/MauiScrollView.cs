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
		bool? _scrollViewDescendant;

		double _lastMeasureHeight;
		double _lastMeasureWidth;
		double _lastArrangeHeight;
		double _lastArrangeWidth;

		UIUserInterfaceLayoutDirection? _previousEffectiveUserInterfaceLayoutDirection;
		SafeAreaPadding _safeArea = SafeAreaPadding.Empty;
		bool _safeAreaInvalidated = true;
		bool _appliesSafeAreaAdjustments;

		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		ICrossPlatformLayout? ICrossPlatformLayoutBacking.CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		internal ICrossPlatformLayout? CrossPlatformLayout => ((ICrossPlatformLayoutBacking)this).CrossPlatformLayout;

		public MauiScrollView()
		{
			ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
		}

		bool RespondsToSafeArea()
		{
			return !(_scrollViewDescendant ??= Superview.GetParentOfType<UIScrollView>() is not null);
		}

		public override void SafeAreaInsetsDidChange()
		{
			// Note: UIKit invokes LayoutSubviews right after this method
			base.SafeAreaInsetsDidChange();

			_safeAreaInvalidated = true;
		}

		public override void LayoutSubviews()
		{
			if (CrossPlatformLayout is null)
			{
				base.LayoutSubviews();
				return;
			}

			if (!ValidateSafeArea())
			{
				InvalidateConstraintsCache();
			}

			// LayoutSubviews is invoked while scrolling, so we need to arrange the content only when it's necessary.
			// This could be done via `override ScrollViewHandler.PlatformArrange` but that wouldn't cover the case
			// when the ScrollView is attached to a non-MauiView parent (i.e. DeviceTests).
			var bounds = Bounds;
			var widthConstraint = (double)bounds.Width;
			var heightConstraint = (double)bounds.Height;
			var frameChanged = _lastArrangeWidth != widthConstraint || _lastArrangeHeight != heightConstraint;

			// If the frame changed, we need to arrange (and potentially measure) the content again
			if (frameChanged)
			{
				_lastArrangeWidth = widthConstraint;
				_lastArrangeHeight = heightConstraint;

				if (!IsMeasureValid(widthConstraint, heightConstraint))
				{
					CrossPlatformMeasure(widthConstraint, heightConstraint);
					CacheMeasureConstraints(widthConstraint, heightConstraint);
				}

				Size crossPlatformBounds;
				// Account for safe area adjustments automatically added by iOS
				var contentSize = CrossPlatformArrange(Bounds, out crossPlatformBounds).ToCGSize();

				// For Right-To-Left (RTL) layouts, we need to adjust the content arrangement and offset
				// to ensure the content is correctly aligned and scrolled. This involves a second layout
				// arrangement with an adjusted starting point and recalculating the content offset.
				if (_previousEffectiveUserInterfaceLayoutDirection is not null && _previousEffectiveUserInterfaceLayoutDirection != EffectiveUserInterfaceLayoutDirection)
				{
					if (EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
					{
						var horizontalOffset = contentSize.Width - crossPlatformBounds.Width;
						CrossPlatformArrange(new Rect(new Point(-horizontalOffset, 0), crossPlatformBounds), out crossPlatformBounds);
						ContentOffset = new CGPoint(horizontalOffset, 0);
					}
					else
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

		bool ValidateSafeArea()
		{
			// If nothing changed, we don't need to do anything
			if (!_safeAreaInvalidated)
			{
				return true;
			}

			// Mark the safe area as validated given that we're about to check it
			_safeAreaInvalidated = false;

			var oldSafeArea = _safeArea;
			_safeArea = SafeAreaInsets.ToSafeAreaInsets();

			var oldApplyingSafeAreaAdjustments = _appliesSafeAreaAdjustments;
			_appliesSafeAreaAdjustments = RespondsToSafeArea() && !_safeArea.IsEmpty;

			// Return whether the way safe area interacts with our view has changed
			return oldApplyingSafeAreaAdjustments == _appliesSafeAreaAdjustments &&
			       (oldSafeArea == _safeArea || !_appliesSafeAreaAdjustments);
		}

		Size CrossPlatformArrange(CGRect bounds, out Size adjustedBounds)
		{
			bounds = new CGRect(CGPoint.Empty, bounds.Size);
			if (_appliesSafeAreaAdjustments)
			{
				bounds = _safeArea.InsetRect(bounds);
			}

			adjustedBounds = bounds.Size.ToSize();

			var size = CrossPlatformLayout?.CrossPlatformArrange(bounds.ToRectangle()) ?? Size.Zero;

			if (_appliesSafeAreaAdjustments)
			{
				size = new Size(size.Width + _safeArea.HorizontalThickness, size.Height + _safeArea.VerticalThickness);
			}

			return size;
		}

		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			if (_appliesSafeAreaAdjustments)
			{
				// When responding to safe area, we need to adjust the constraints to account for the safe area.
				widthConstraint -= _safeArea.HorizontalThickness;
				heightConstraint -= _safeArea.VerticalThickness;
			}

			var crossPlatformSize = CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;

			if (_appliesSafeAreaAdjustments)
			{
				// If we're responding to the safe area, we need to add the safe area back to the size so the container can allocate the correct space
				crossPlatformSize = new Size(crossPlatformSize.Width + _safeArea.HorizontalThickness, crossPlatformSize.Height + _safeArea.VerticalThickness);
			}

			return crossPlatformSize;
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (CrossPlatformLayout is null)
			{
				return new CGSize();
			}

			var widthConstraint = (double)size.Width;
			var heightConstraint = (double)size.Height;

			var contentSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

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
			_scrollViewDescendant = null;
			_safeAreaInvalidated = true;

			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}
	}
}