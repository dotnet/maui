using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// A custom UIScrollView implementation that provides cross-platform layout support and safe area management
	/// for .NET MAUI applications on iOS. This class handles the bridge between MAUI's cross-platform layout
	/// system and iOS's native UIScrollView behavior.
	/// </summary>
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, ICrossPlatformLayoutBacking, IPlatformMeasureInvalidationController
	{
		/// <summary>
		/// Flag indicating whether the parent view hierarchy should be invalidated when this view is moved to a window.
		/// Used to ensure proper layout recalculation when the view becomes visible.
		/// </summary>
		bool _invalidateParentWhenMovedToWindow;
		
		/// <summary>
		/// Cached result of whether this scroll view is a descendant of another UIScrollView.
		/// Null when not yet calculated, true if nested within another scroll view, false otherwise.
		/// This affects safe area handling behavior.
		/// </summary>
		bool? _scrollViewDescendant;

		/// <summary>
		/// The height constraint used in the last measure operation.
		/// Used to determine if a re-measure is needed when constraints change.
		/// </summary>
		double _lastMeasureHeight;
		
		/// <summary>
		/// The width constraint used in the last measure operation.
		/// Used to determine if a re-measure is needed when constraints change.
		/// </summary>
		double _lastMeasureWidth;
		
		/// <summary>
		/// The height constraint used in the last arrange operation.
		/// Used to determine if a re-arrange is needed when the frame changes.
		/// </summary>
		double _lastArrangeHeight;
		
		/// <summary>
		/// The width constraint used in the last arrange operation.
		/// Used to determine if a re-arrange is needed when the frame changes.
		/// </summary>
		double _lastArrangeWidth;

		/// <summary>
		/// The current safe area padding values derived from iOS's AdjustedContentInset.
		/// Represents the areas that should be avoided when placing content (e.g., status bar, home indicator).
		/// </summary>
		SafeAreaPadding _safeArea = SafeAreaPadding.Empty;

		UIEdgeInsets _systemAdjustedContentInset = UIEdgeInsets.Zero;
		
		/// <summary>
		/// Flag indicating whether the safe area needs to be recalculated.
		/// Set to true when iOS notifies us of safe area changes.
		/// </summary>
		bool _safeAreaInvalidated = true;
		
		/// <summary>
		/// Flag indicating whether this scroll view should apply safe area adjustments to its content.
		/// Only true when not nested in another scroll view and safe area is not empty.
		/// </summary>
		bool _appliesSafeAreaAdjustments;
		
		/// <summary>
		/// The previous effective user interface layout direction (LTR/RTL).
		/// Used to detect when the layout direction changes and trigger appropriate content repositioning.
		/// </summary>
		UIUserInterfaceLayoutDirection? _previousEffectiveUserInterfaceLayoutDirection;

		/// <summary>
		/// Weak reference to the cross-platform layout that manages the content of this scroll view.
		/// Weak reference prevents circular references and allows proper garbage collection.
		/// </summary>
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		/// <summary>
		/// Gets or sets the cross-platform layout that manages the content of this scroll view.
		/// The layout is responsible for measuring and arranging the scroll view's content.
		/// </summary>
		ICrossPlatformLayout? ICrossPlatformLayoutBacking.CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		/// <summary>
		/// Internal accessor for the cross-platform layout. Provides typed access to the layout manager.
		/// </summary>
		internal ICrossPlatformLayout? CrossPlatformLayout => ((ICrossPlatformLayoutBacking)this).CrossPlatformLayout;

		/// <summary>
		/// Initializes a new instance of the MauiScrollView class.
		/// </summary>
		public MauiScrollView()
		{
		}

		/// <summary>
		/// Determines whether this scroll view should respond to safe area changes.
		/// Returns false if this scroll view is nested within another scroll view,
		/// as nested scroll views should not apply their own safe area adjustments.
		/// </summary>
		/// <returns>True if this scroll view should apply safe area adjustments, false otherwise.</returns>
		bool RespondsToSafeArea()
		{
			return !(_scrollViewDescendant ??= Superview.GetParentOfType<UIScrollView>() is not null);
		}

		/// <summary>
		/// Called by iOS when the adjusted content inset changes (e.g., when safe area changes).
		/// This method invalidates the safe area and triggers a layout update if needed.
		/// </summary>
		public override void AdjustedContentInsetDidChange()
		{
			base.AdjustedContentInsetDidChange();
			_safeAreaInvalidated = true;

			// It looks like when this invalidates it doesn't auto trigger a layout pass
			if (!ValidateSafeArea())
			{
				((IPlatformMeasureInvalidationController)this).InvalidateMeasure();
				this.InvalidateAncestorsMeasures();
			}
		}

		/// <summary>
		/// Called by iOS when the safe area insets change (e.g., device rotation, notch visibility).
		/// This method marks the safe area as invalidated. Note that UIKit automatically calls
		/// LayoutSubviews immediately after this method.
		/// </summary>
		public override void SafeAreaInsetsDidChange()
		{
			// Note: UIKit invokes LayoutSubviews right after this method
			base.SafeAreaInsetsDidChange();

			_safeAreaInvalidated = true;
		}

		/// <summary>
		/// Overrides the default UIScrollView layout behavior to integrate with MAUI's cross-platform layout system.
		/// This method handles safe area validation, measures and arranges content, and manages RTL layout adjustments.
		/// It's called by iOS whenever the view needs to be laid out, including during scrolling operations.
		/// </summary>
		public override void LayoutSubviews()
		{
			// If there's no cross-platform layout, fall back to default UIScrollView behavior
			if (CrossPlatformLayout is null)
			{
				base.LayoutSubviews();
				return;
			}

			// Validate and update safe area if needed, invalidating constraints cache if changes occurred
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

				// Check if we need to re-measure the content with the new constraints
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
						CrossPlatformArrange(new Rect(new Point(-horizontalOffset, 0), crossPlatformBounds), out _);
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

		/// <summary>
		/// Validates and updates the safe area configuration. This method checks if the safe area
		/// has changed and updates the internal state accordingly.
		/// </summary>
		/// <returns>True if the safe area configuration hasn't changed in a way that affects layout, false otherwise.</returns>
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

			// iOS sets AdjustedContentInset only when the ContentSize exceeds the ScrollView's Bounds.
			// If ContentSize is smaller, AdjustedContentInset is zero, and SafeAreaInsets are applied to child views instead.
			// This makes sense for non-scrolling scenarios, but creates a problem: if a child view's height increases due to safe area,
			// it can push ContentSize over the Bounds, causing AdjustedContentInset to become non-zero and SafeAreaInsets on the child to reset to zero.
			// This can result in a loop of invalidations as the layout toggles between these states.
			// To prevent this, we ignore safe area calculations on child views when they are inside a scroll view.
			if (SystemAdjustedContentInset == UIEdgeInsets.Zero)
				_safeArea = SafeAreaInsets.ToSafeAreaInsets();
			else
				_safeArea = AdjustedContentInset.ToSafeAreaInsets();

			var oldApplyingSafeAreaAdjustments = _appliesSafeAreaAdjustments;
			_appliesSafeAreaAdjustments = RespondsToSafeArea() && !_safeArea.IsEmpty;

			if (_systemAdjustedContentInset != SystemAdjustedContentInset)
			{
				_systemAdjustedContentInset = SystemAdjustedContentInset;
				return false;
			}

			// Return whether the way safe area interacts with our view has changed
			return oldApplyingSafeAreaAdjustments == _appliesSafeAreaAdjustments &&
			       (oldSafeArea == _safeArea || !_appliesSafeAreaAdjustments);
		}

		UIEdgeInsets SystemAdjustedContentInset
		{
			get
			{
				UIEdgeInsets adjusted = AdjustedContentInset;
				UIEdgeInsets content = ContentInset;

				return new UIEdgeInsets(
					adjusted.Top - content.Top,
					adjusted.Left - content.Left,
					adjusted.Bottom - content.Bottom,
					adjusted.Right - content.Right
				);
			}
		}

		/// <summary>
		/// Arranges the cross-platform content within the specified bounds, accounting for safe area adjustments.
		/// This method applies safe area insets to the bounds before arranging the content.
		/// </summary>
		/// <param name="bounds">The bounds within which to arrange the content.</param>
		/// <param name="adjustedBounds">The bounds after safe area adjustments have been applied.</param>
		/// <returns>The size of the arranged content.</returns>
		Size CrossPlatformArrange(CGRect bounds, out Size adjustedBounds)
		{
			bounds = new Rect(new Point(), bounds.Size.ToSize());
			// Apply safe area adjustments to the bounds if this scroll view responds to safe area
			if (_appliesSafeAreaAdjustments)
			{
				bounds = _safeArea.InsetRect(bounds);
			}

			adjustedBounds = bounds.Size.ToSize();

			Size size;

			if (SystemAdjustedContentInset == UIEdgeInsets.Zero)
				size = CrossPlatformLayout?.CrossPlatformArrange(bounds.ToRectangle()) ?? Size.Zero;
			else
				size = CrossPlatformLayout?.CrossPlatformArrange(new Rect(new Point(), bounds.Size.ToSize())) ?? Size.Zero;

			return size;
		}

		/// <summary>
		/// Measures the cross-platform content with the specified constraints, accounting for safe area adjustments.
		/// This method reduces the constraints by the safe area thickness before measuring, then adds it back
		/// to the result so the container can allocate the correct space.
		/// </summary>
		/// <param name="widthConstraint">The available width for the content.</param>
		/// <param name="heightConstraint">The available height for the content.</param>
		/// <returns>The measured size of the content, including safe area adjustments if applicable.</returns>
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

		/// <summary>
		/// Calculates the size that fits within the given constraints. This method is called by iOS
		/// when the system needs to determine the natural size of the scroll view.
		/// </summary>
		/// <param name="size">The available size constraints.</param>
		/// <returns>The size that fits within the constraints.</returns>
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

		/// <summary>
		/// Marks that ancestor measures should be invalidated when this view is moved to a window.
		/// This is used to ensure proper layout recalculation when the view becomes visible.
		/// </summary>
		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		/// <summary>
		/// Invalidates the measure of this view, causing it to be re-measured and re-laid out.
		/// This method is called when the view's content or constraints change.
		/// </summary>
		/// <param name="isPropagating">Whether this invalidation is propagating up the view hierarchy.</param>
		/// <returns>True if the invalidation should stop propagating, false otherwise.</returns>
		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			SetNeedsLayout();
			InvalidateConstraintsCache();

			return !isPropagating;
		}

		/// <summary>
		/// Checks if the current measure is valid for the given constraints.
		/// This helps avoid unnecessary re-measurements when the constraints haven't changed.
		/// </summary>
		/// <param name="widthConstraint">The width constraint to check.</param>
		/// <param name="heightConstraint">The height constraint to check.</param>
		/// <returns>True if the last measure is still valid for these constraints, false otherwise.</returns>
		bool IsMeasureValid(double widthConstraint, double heightConstraint)
		{
			// Check the last constraints this View was measured with; if they're the same,
			// then the current measure info is already correct, and we don't need to repeat it
			return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
		}

		/// <summary>
		/// Invalidates the cached constraint values, forcing a re-measurement and re-arrangement
		/// on the next layout pass.
		/// </summary>
		void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
			_lastArrangeWidth = double.NaN;
			_lastArrangeHeight = double.NaN;
		}

		/// <summary>
		/// Caches the measure constraints for future validation.
		/// This helps optimize performance by avoiding unnecessary re-measurements.
		/// </summary>
		/// <param name="widthConstraint">The width constraint to cache.</param>
		/// <param name="heightConstraint">The height constraint to cache.</param>
		void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
		}

		/// <summary>
		/// Overrides the default scroll-to-visible behavior to prevent automatic scrolling
		/// when the KeyboardAutoManagerScroll is handling keyboard-related scrolling.
		/// This prevents conflicts between manual keyboard scrolling and automatic UITextField scrolling.
		/// </summary>
		/// <param name="rect">The rectangle to scroll to.</param>
		/// <param name="animated">Whether the scrolling should be animated.</param>
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
			if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
				base.ScrollRectToVisible(rect, animated);
		}

		/// <summary>
		/// Event handler for the MovedToWindow event. This is used to support the IUIViewLifeCycleEvents interface
		/// and allows subscribers to be notified when the view hierarchy changes.
		/// </summary>
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;

		/// <summary>
		/// Event that is raised when this view is moved to a window (added to or removed from the view hierarchy).
		/// </summary>
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		/// <summary>
		/// Called when the view is moved to a window (added to or removed from the view hierarchy).
		/// This method handles cleanup and initialization tasks, including invalidating cached values
		/// and triggering ancestor measure invalidation if needed.
		/// </summary>
		public override void MovedToWindow()
		{
			base.MovedToWindow();

			// Notify any subscribers that the view has been moved to a window
			_movedToWindow?.Invoke(this, EventArgs.Empty);
			
			// Clear cached scroll view descendant status since the view hierarchy may have changed
			_scrollViewDescendant = null;
			
			// Mark safe area as invalidated since moving to a new window may change safe area
			_safeAreaInvalidated = true;

			// If ancestor measure invalidation was requested, trigger it now
			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}
	}
}