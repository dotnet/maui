using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
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
		internal const nint ContentTag = 0x845fed;

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
		/// The previous scroll orientation.
		/// Used to detect when the scroll orientation changes and trigger appropriate RTL repositioning.
		/// </summary>
		ScrollOrientation? _previousScrollOrientation;

		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;


		/// <summary>
		/// Weak reference to the cross-platform layout that manages the content of this scroll view.
		/// Weak reference prevents circular references and allows proper garbage collection.
		/// </summary>
		WeakReference<IView>? _reference;

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
		/// Gets the current scroll orientation from the cross-platform view.
		/// </summary>
		/// <returns>The current scroll orientation, or null if not available.</returns>
		ScrollOrientation? GetCurrentScrollOrientation()
		{
			// Access the scroll orientation from the view if it's a ScrollView
			if (View is IScrollView scrollView)
			{
				return scrollView.Orientation;
			}
			return null;
		}

		/// <summary>
		/// Called when the scroll orientation has changed to trigger proper RTL layout recalculation.
		/// </summary>
		internal void OnOrientationChanged()
		{
			// Reset the previous orientation to force re-evaluation of RTL layout
			_previousScrollOrientation = null;
			SetNeedsLayout();
			LayoutIfNeeded();
		}

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
		internal IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		SafeAreaRegions GetSafeAreaRegionForEdge(int edge)
		{
			if (View is ISafeAreaView2 safeAreaPage)
			{
				return safeAreaPage.GetSafeAreaRegionsForEdge(edge);
			}
			
			return SafeAreaRegions.None; // Default: edge-to-edge content
		}

		SafeAreaEdges? _previousEdges;

		UIEdgeInsets GetInset()
		{
			var leftRegion = GetSafeAreaRegionForEdge(0);
			var topRegion = GetSafeAreaRegionForEdge(1);
			var rightRegion = GetSafeAreaRegionForEdge(2);
			var bottomRegion = GetSafeAreaRegionForEdge(3);

			var safeAreaInsets = SafeAreaInsets;

			var manualInset = new UIEdgeInsets(
					top: GetManualInsetForEdge(topRegion, safeAreaInsets.Top),
					left: GetManualInsetForEdge(leftRegion, safeAreaInsets.Left),
					bottom: GetManualInsetForEdge(bottomRegion, safeAreaInsets.Bottom),
					right: GetManualInsetForEdge(rightRegion, safeAreaInsets.Right)
				);

			return manualInset;
		}

		bool UpdateContentInsetAdjustmentBehavior()
		{
			// Get SafeAreaRegions for all edges
			var leftRegion = GetSafeAreaRegionForEdge(0);
			var topRegion = GetSafeAreaRegionForEdge(1);
			var rightRegion = GetSafeAreaRegionForEdge(2);
			var bottomRegion = GetSafeAreaRegionForEdge(3);

			SafeAreaEdges safeAreaEdges = new SafeAreaEdges(leftRegion, topRegion, rightRegion, bottomRegion);

			if (_previousEdges is not null && _previousEdges.Equals(safeAreaEdges))
				return false;

			_previousEdges = safeAreaEdges;

			// Check if all edges have the same SafeAreaRegions value
			if (leftRegion == topRegion && topRegion == rightRegion && rightRegion == bottomRegion)
			{
				// All edges have the same value, use built-in iOS behavior
				// Cache the region value to avoid redundant comparisons
				var region = leftRegion;
				
				ContentInsetAdjustmentBehavior = region switch
				{
					SafeAreaRegions.Default => UIScrollViewContentInsetAdjustmentBehavior.Automatic, // Default behavior
					SafeAreaRegions.None => UIScrollViewContentInsetAdjustmentBehavior.Never, // Edge-to-edge content
					SafeAreaRegions.All => UIScrollViewContentInsetAdjustmentBehavior.Never, // We calculate insets ourselves and include keyboard
					_ when SafeAreaEdges.IsContainer(region) => UIScrollViewContentInsetAdjustmentBehavior.Always, // Content flows under keyboard but stays out of bars/notch
					_ when SafeAreaEdges.IsSoftInput(region) => UIScrollViewContentInsetAdjustmentBehavior.Never, // We calculate insets ourselves and include keyboard
					_ => UIScrollViewContentInsetAdjustmentBehavior.Never // Default: edge-to-edge
				};
			}
			else
			{
				// Mixed edges - use manual calculation
				ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
			}

			return true;
		}

		static nfloat GetManualInsetForEdge(SafeAreaRegions safeAreaRegion, nfloat safeAreaInset)
		{
			// Edge-to-edge content - no safe area padding
			if (safeAreaRegion == SafeAreaRegions.None)
				return 0;

			return safeAreaInset;
		}

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
			ValidateSafeArea();
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

				var contentSize = CrossPlatformArrange(Bounds).ToCGSize();

				// Clamp content size based on ScrollView orientation to prevent unwanted scrolling
				if (View is IScrollView scrollView)
				{
					var frameSize = Bounds.Size;
					var orientation = scrollView.Orientation;

					// Clamp width if horizontal scrolling is disabled and content is larger than frame
					if (orientation is ScrollOrientation.Vertical or ScrollOrientation.Neither && contentSize.Width > frameSize.Width)
					{
						contentSize = new CGSize(frameSize.Width, contentSize.Height);
					}

					// Clamp height if vertical scrolling is disabled and content is larger than frame
					if (orientation is ScrollOrientation.Horizontal or ScrollOrientation.Neither && contentSize.Height > frameSize.Height)
					{
						contentSize = new CGSize(contentSize.Width, frameSize.Height);
					}
				}

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
			//UpdateKeyboardSubscription();
			// If nothing changed, we don't need to do anything

			if (!UpdateContentInsetAdjustmentBehavior())
			{
				InvalidateConstraintsCache();
				_safeAreaInvalidated = true;
			}

			if (!_safeAreaInvalidated)
			{
				return true;
			}

			// Mark the safe area as validated given that we're about to check it
			_safeAreaInvalidated = true;

			var oldSafeArea = _safeArea;

			// iOS sets AdjustedContentInset only when the ContentSize exceeds the ScrollView's Bounds.
			// If ContentSize is smaller, AdjustedContentInset is zero, and SafeAreaInsets are applied to child views instead.
			// This makes sense for non-scrolling scenarios, but creates a problem: if a child view's height increases due to safe area,
			// it can push ContentSize over the Bounds, causing AdjustedContentInset to become non-zero and SafeAreaInsets on the child to reset to zero.
			// This can result in a loop of invalidations as the layout toggles between these states.
			// To prevent this, we ignore safe area calculations on child views when they are inside a scroll view.
			if (SystemAdjustedContentInset == UIEdgeInsets.Zero || ContentInsetAdjustmentBehavior == UIScrollViewContentInsetAdjustmentBehavior.Never)
				_safeArea = GetInset().ToSafeAreaInsets();
			else
				_safeArea = SystemAdjustedContentInset.ToSafeAreaInsets();

			var oldApplyingSafeAreaAdjustments = _appliesSafeAreaAdjustments;
			_appliesSafeAreaAdjustments = RespondsToSafeArea() && !_safeArea.IsEmpty;

			if (_systemAdjustedContentInset != SystemAdjustedContentInset)
			{
				InvalidateConstraintsCache();
				_systemAdjustedContentInset = SystemAdjustedContentInset;
				return false;
			}

			if (!oldSafeArea.Equals(_safeArea))
			{
				InvalidateConstraintsCache();
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
		/// <returns>The size of the arranged content.</returns>
		Size CrossPlatformArrange(CGRect bounds)
		{
			bounds = new Rect(new Point(), bounds.Size.ToSize());
			// Apply safe area adjustments to the bounds if this scroll view responds to safe area
			if (_appliesSafeAreaAdjustments)
			{
				bounds = _safeArea.InsetRect(bounds);
			}

			Size contentSize;


			double width;
			double height;
			if (SystemAdjustedContentInset == UIEdgeInsets.Zero || ContentInsetAdjustmentBehavior == UIScrollViewContentInsetAdjustmentBehavior.Never)
			{
				contentSize = CrossPlatformLayout?.CrossPlatformArrange(bounds.ToRectangle()) ?? Size.Zero;

				width = contentSize.Width;
				height = contentSize.Height;
			}
			else
			{
				contentSize = CrossPlatformLayout?.CrossPlatformArrange(new Rect(new Point(), bounds.Size.ToSize())) ?? Size.Zero;

				width = contentSize.Width;
				height = contentSize.Height;
			}


			// When using ContentInsetAdjustmentBehavior.Automatic, UIKit dynamically decides whether to apply 
			// safe area insets to the scroll view (via AdjustedContentInset) or to push them into the child view's SafeAreaInsets.
			// This decision depends on whether the scroll view is considered "scrollable"—i.e., whether the contentSize 
			// is larger than the visible bounds (after accounting for safe areas).
			//
			// If the content size is *just* smaller than or equal to the scroll view’s bounds, UIKit may decide that
			// scrolling isn’t needed and push the safe area insets into the child instead. This can cause:
			//   - content centering to behave incorrectly (e.g., not respecting safe areas),
			//   - layout loops where the child resizes in response to changing safe area insets,
			//   - instability when transitioning between scrollable and non-scrollable states.
			//
			// This logic adds safe area padding to the contentSize *only if* the content is nearly large enough to require scrolling,
			// ensuring the scroll view remains in "scrollable mode" and keeps safe area insets at the scroll view level.
			// This avoids inset flip-flopping and keeps layout behavior stable and predictable.
			if (ContentInsetAdjustmentBehavior == UIScrollViewContentInsetAdjustmentBehavior.Automatic)
			{
				// We do this to keep the content scrollable
				// if we don't do this the ContentAdjustedInset + contentSize will cause the content to go off the screen and not be scrollable
				// So the bottom content will just go off the screen until the contentsize triggers the scrollable area
				if (width <= Bounds.Width &&
					(_safeArea.HorizontalThickness + width) > Bounds.Width)
				{
					width += Bounds.Width + 1;
				}

				if (height <= Bounds.Height &&
					(_safeArea.VerticalThickness + height) > Bounds.Height)
				{
					height = Bounds.Height + 1;
				}
			}
			else if (ContentInsetAdjustmentBehavior != UIScrollViewContentInsetAdjustmentBehavior.Automatic)
			{
				width += _safeArea.HorizontalThickness;
				height += _safeArea.VerticalThickness;
			}

			contentSize = new Size(width, height);

			// Check if the orientation has changed
			var currentScrollOrientation = GetCurrentScrollOrientation();
			bool orientationChanged = _previousScrollOrientation != currentScrollOrientation;

			// For Right-To-Left (RTL) layouts, we need to adjust the content arrangement and offset
			// to ensure the content is correctly aligned and scrolled. This involves a second layout
			// arrangement with an adjusted starting point and recalculating the content offset.
			// We also need to handle this when the orientation changes from vertical to horizontal
			// while staying in RTL mode.
			if (_previousEffectiveUserInterfaceLayoutDirection != EffectiveUserInterfaceLayoutDirection || 
			    (orientationChanged && EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft))
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
					var horizontalOffset = contentSize.Width - bounds.Width;
					
					if (SystemAdjustedContentInset == UIEdgeInsets.Zero || ContentInsetAdjustmentBehavior == UIScrollViewContentInsetAdjustmentBehavior.Never)
					{
						CrossPlatformLayout?.CrossPlatformArrange(new Rect(new Point(-horizontalOffset, 0), bounds.Size.ToSize()));
					}
					else
					{
						CrossPlatformLayout?.CrossPlatformArrange(new Rect(new Point(-horizontalOffset, 0), bounds.Size.ToSize()));
					}
					
					ContentOffset = new CGPoint(horizontalOffset, 0);

				}
				else if(_previousEffectiveUserInterfaceLayoutDirection is not null)
				{
					ContentOffset = new CGPoint(0, ContentOffset.Y);
				}
			}

			// When switching between LTR and RTL, we need to re-arrange and offset content exactly once
			// to avoid cumulative shifts or incorrect offsets on subsequent layouts.
			_previousEffectiveUserInterfaceLayoutDirection = EffectiveUserInterfaceLayoutDirection;
			_previousScrollOrientation = currentScrollOrientation;

			return contentSize;
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
			ValidateSafeArea();
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