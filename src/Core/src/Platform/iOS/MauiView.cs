using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Base class for MAUI views on iOS that provides cross-platform layout capabilities
	/// and safe area handling. This view bridges the gap between iOS native UIView
	/// and MAUI's cross-platform layout system.
	/// </summary>
	public abstract class MauiView : UIView, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, IUIViewLifeCycleEvents, IPlatformMeasureInvalidationController
	{
		/// <summary>
		/// Tracks whether this view currently has native iOS focus.
		/// Used to synchronize focus state between the platform and cross-platform layers.
		/// </summary>
		bool _isFocused;

		/// <summary>
		/// Flag indicating that parent views should be invalidated when this view is moved to a window.
		/// This is used to trigger layout updates when the view hierarchy changes.
		/// </summary>
		bool _invalidateParentWhenMovedToWindow;

		/// <summary>
		/// Cached height constraint from the last measure operation.
		/// Used to avoid redundant measure calls when constraints haven't changed.
		/// NaN indicates no previous measure has been performed.
		/// </summary>
		double _lastMeasureHeight = double.NaN;

		/// <summary>
		/// Cached width constraint from the last measure operation.
		/// Used to avoid redundant measure calls when constraints haven't changed.
		/// NaN indicates no previous measure has been performed.
		/// </summary>
		double _lastMeasureWidth = double.NaN;

		/// <summary>
		/// Cached measured cross-platform size from the last measure operation tight to <see cref="_lastMeasureWidth"/> and <see cref="_lastMeasureHeight"/>.
		/// Used to avoid redundant measure calls when constraints haven't changed.
		/// NaN indicates no previous measure has been performed.
		/// </summary>
		Size? _lastMeasuredSize;

		/// <summary>
		/// Current safe area padding values (top, left, bottom, right) in device-independent units.
		/// These values represent the insets needed to avoid system UI elements like status bars,
		/// navigation bars, and home indicators.
		/// </summary>
		SafeAreaPadding _safeArea = SafeAreaPadding.Empty;

		/// <summary>
		/// Flag indicating that the safe area has changed and needs to be revalidated.
		/// Set to true when SafeAreaInsetsDidChange() is called by the system.
		/// </summary>
		bool _safeAreaInvalidated = true;

		/// <summary>
		/// Flag indicating whether this view should apply safe area adjustments to its layout.
		/// This is true when the view implements ISafeAreaView, doesn't ignore safe area,
		/// and the safe area is not empty.
		/// </summary>
		bool _appliesSafeAreaAdjustments;

		// Indicates whether this view should respond to safe area insets.
		// Cached to avoid repeated hierarchy checks.
		// True if the view is an ISafeAreaView, does not ignore safe area, and is not inside a UIScrollView;
		// otherwise, false. Null means not yet determined.
		bool? _scrollViewDescendant;

		// Keyboard tracking
		CGRect _keyboardFrame = CGRect.Empty;
		bool _isKeyboardShowing;
		WeakReference<NSObject>? _keyboardWillShowObserver;
		WeakReference<NSObject>? _keyboardWillHideObserver;

		/// <summary>
		/// Weak reference to the cross-platform IView that this native view represents.
		/// Uses WeakReference to avoid circular references between platform and cross-platform layers.
		/// </summary>
		WeakReference<IView>? _reference;

		/// <summary>
		/// Weak reference to the cross-platform layout manager for this view.
		/// Used to delegate measure and arrange operations to the cross-platform layout system.
		/// </summary>
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		/// <summary>
		/// Gets or sets the cross-platform IView that this native view represents.
		/// This provides access to the MAUI view properties and behavior from the platform layer.
		/// </summary>
		public IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		/// <summary>
		/// Determines if this view has fixed constraints that prevent it from changing size.
		/// Views with fixed constraints don't need to propagate measure invalidations to ancestors.
		/// </summary>
		bool HasFixedConstraints => CrossPlatformLayout is IConstrainedView { HasFixedConstraints: true };

		/// <summary>
		/// Determines if this view should respond to safe area changes.
		/// Returns true if the view implements ISafeAreaView, doesn't ignore safe area,
		/// and the current iOS version supports safe area insets.
		/// </summary>
		bool RespondsToSafeArea()
		{
			if (_scrollViewDescendant.HasValue)
				return !_scrollViewDescendant.Value;

			// iOS sets AdjustedContentInset on UIScrollView only when the ContentSize exceeds the ScrollView's Bounds.
			// If ContentSize is smaller, AdjustedContentInset is zero, and SafeAreaInsets are applied to child views instead.
			// This makes sense for non-scrolling scenarios, but creates a problem: if a child view's height increases due to safe area,
			// it can push ContentSize over the Bounds, causing AdjustedContentInset to become non-zero and SafeAreaInsets on the child to reset to zero.
			// This can result in a loop of invalidations as the layout toggles between these states (child applies safe area, triggers scrollview to adjust, which removes safe area from child, and so on).
			//
			// To prevent this, we ignore safe area calculations on child views when they are inside a scroll view.
			// The scrollview itself is responsible for applying the correct insets, and child views should not apply additional safe area logic.
			//
			// For more details and implementation specifics, see MauiScrollView.cs, which contains the logic for safe area management
			// within scroll views and explains how this interacts with the overall layout system.
			_scrollViewDescendant = this.GetParentOfType<UIScrollView>() is not null;
			return !_scrollViewDescendant.Value;
		}

		SafeAreaRegions GetSafeAreaRegionForEdge(int edge)
		{
			if (View is ISafeAreaView2 safeAreaPage)
			{
				return safeAreaPage.GetSafeAreaRegionsForEdge(edge);
			}

			// Fallback to legacy ISafeAreaView behavior
			if (View is ISafeAreaView sav)
			{
				return sav.IgnoreSafeArea ? SafeAreaRegions.None : SafeAreaRegions.Container;
			}

			return SafeAreaRegions.None;
		}

		static double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea)
		{
			// Edge-to-edge content - no safe area padding
			if (safeAreaRegion == SafeAreaRegions.None)
				return 0;

			// All other regions respect safe area in some form
			// This includes:
			// - Default: Platform default behavior
			// - All: Obey all safe area insets  
			// - SoftInput: Always pad for keyboard/soft input
			// - Container: Content flows under keyboard but stays out of bars/notch
			// - Any combination of the above flags
			return originalSafeArea;
		}


		/// <summary>
		/// Adjusts the given bounds rectangle to account for safe area insets.
		/// This method subtracts the safe area padding from the bounds to ensure
		/// content doesn't overlap with system UI elements.
		/// </summary>
		/// <param name="bounds">The original bounds rectangle</param>
		/// <returns>The bounds rectangle adjusted for safe area insets</returns>
		protected CGRect AdjustForSafeArea(CGRect bounds)
		{
			// Special handling for keyboard auto-scroll scenarios
			if (KeyboardAutoManagerScroll.ShouldIgnoreSafeAreaAdjustment)
			{
				KeyboardAutoManagerScroll.ShouldScrollAgain = true;
			}

			ValidateSafeArea();
			return _safeArea.InsetRect(bounds);
		}

		bool ShouldSubscribeToKeyboardNotifications()
		{
			// Only subscribe if any edge has All or SoftInput regions
			if (View is ISafeAreaView2 safeAreaPage)
			{
				for (int edge = 0; edge < 4; edge++)
				{
					var region = safeAreaPage.GetSafeAreaRegionsForEdge(edge);
					if (SafeAreaEdges.IsSoftInput(region))
					{
						return true;
					}
				}
			}
			return false;
		}

		void SubscribeToKeyboardNotifications()
		{
			if (_keyboardWillShowObserver != null || _keyboardWillHideObserver != null)
			{
				// Already subscribed, no need to re-subscribe
				return;
			}

			var showObserver = NSNotificationCenter.DefaultCenter.AddObserver(
				UIKeyboard.WillShowNotification,
				OnKeyboardWillShow);
			_keyboardWillShowObserver = new WeakReference<NSObject>(showObserver);

			var hideObserver = NSNotificationCenter.DefaultCenter.AddObserver(
				UIKeyboard.WillHideNotification,
				OnKeyboardWillHide);
			_keyboardWillHideObserver = new WeakReference<NSObject>(hideObserver);
		}

		void UnsubscribeFromKeyboardNotifications()
		{
			if (_keyboardWillShowObserver?.TryGetTarget(out var showObserver) == true)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(showObserver);
				_keyboardWillShowObserver = null;
			}

			if (_keyboardWillHideObserver?.TryGetTarget(out var hideObserver) == true)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(hideObserver);
				_keyboardWillHideObserver = null;
			}
		}

		void UpdateKeyboardSubscription()
		{
			// Update keyboard subscription based on current SafeAreaEdges settings
			if (Window != null)
			{
				if (ShouldSubscribeToKeyboardNotifications())
				{
					SubscribeToKeyboardNotifications();
				}
				else
				{
					UnsubscribeFromKeyboardNotifications();
				}
			}
		}

		void OnKeyboardWillShow(NSNotification notification)
		{
			_safeAreaInvalidated = true;
			var keyboardFrame = GetKeyboardFrame(notification);
			if (keyboardFrame.HasValue)
			{
				_keyboardFrame = keyboardFrame.Value;
				_isKeyboardShowing = true;
				SetNeedsLayout();
			}
		}

		void OnKeyboardWillHide(NSNotification notification)
		{
			_safeAreaInvalidated = true;
			_keyboardFrame = CGRect.Empty;
			_isKeyboardShowing = false;
			SetNeedsLayout();
		}

		static CGRect? GetKeyboardFrame(NSNotification notification)
		{
			if (notification.UserInfo?[UIKeyboard.FrameEndUserInfoKey] is NSValue frameValue)
			{
				return frameValue.CGRectValue;
			}
			return null;
		}

		SafeAreaPadding GetAdjustedSafeAreaInsets()
		{
			var baseSafeArea = SafeAreaInsets.ToSafeAreaInsets();

			// Check if keyboard-aware safe area adjustments are needed
			if (View is ISafeAreaView2 safeAreaPage && _isKeyboardShowing)
			{
				// Check if any edge has SafeAreaRegions.SoftInput set
				var needsKeyboardAdjustment = false;
				for (int edge = 0; edge < 4; edge++)
				{

					var safeAreaRegion = safeAreaPage.GetSafeAreaRegionsForEdge(edge);
					if (SafeAreaEdges.IsSoftInput(safeAreaRegion))
					{
						needsKeyboardAdjustment = true;
						break;
					}
				}

				if (needsKeyboardAdjustment)
				{
					// Get the keyboard frame and calculate its intersection with the current window
					var window = this.Window;

					if (window != null && !_keyboardFrame.IsEmpty)
					{
						var windowFrame = window.Frame;
						var keyboardIntersection = CGRect.Intersect(_keyboardFrame, windowFrame);

						// If keyboard is visible and intersects with window
						if (!keyboardIntersection.IsEmpty)
						{
							var bottomEdgeRegion = safeAreaPage.GetSafeAreaRegionsForEdge(3); // 3 = bottom edge

							// For SafeAreaRegions.SoftInput: Always pad so content doesn't go under the keyboard
							// Bottom edge is most commonly affected by keyboard
							if (SafeAreaEdges.IsSoftInput(bottomEdgeRegion) && !IsSoftInputHandledByParent(this))
							{
								// Use the larger of the current bottom safe area or the keyboard height
								// Get the input control's bottom Y in window coordinates
								var inputBottomY = 0.0;
								if (Window is not null)
								{
									var viewFrameInWindow = this.Superview?.ConvertRectToView(this.Frame, Window) ?? this.Frame;
									inputBottomY = viewFrameInWindow.Y + viewFrameInWindow.Height;
								}
								var keyboardTopY = _keyboardFrame.Y;
								var overlap = inputBottomY > keyboardTopY ? (inputBottomY - keyboardTopY) : 0.0;

								var adjustedBottom = (overlap > 0) ? overlap : baseSafeArea.Bottom;
								baseSafeArea = new SafeAreaPadding(baseSafeArea.Left, baseSafeArea.Right, baseSafeArea.Top, adjustedBottom);
							}
						}
					}
				}
			}

			if (View is ISafeAreaView2)
			{
				// Apply safe area selectively per edge based on SafeAreaRegions
				var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0), baseSafeArea.Left);
				var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1), baseSafeArea.Top);
				var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2), baseSafeArea.Right);
				var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3), baseSafeArea.Bottom);

				return new SafeAreaPadding(left, right, top, bottom);
			}

			// Fallback to legacy behavior
			if (View is ISafeAreaView sav && sav.IgnoreSafeArea)
			{
				return SafeAreaPadding.Empty;
			}

			return baseSafeArea;
		}

		/// <summary>
		/// Checks if any parent view in the hierarchy is a MauiView that implements ISafeAreaView2
		/// and has SafeAreaEdges.SoftInput set for the bottom edge. This is used to determine if
		/// keyboard overlap/padding is already being handled by an ancestor, so the current view
		/// should not apply additional adjustments.
		/// Returns true if a parent is handling soft input, false otherwise.
		/// </summary>
		internal static bool IsSoftInputHandledByParent(UIView view)
		{
			return view.FindParent(x => x is MauiView mv
				&& mv.View is ISafeAreaView2 safeAreaView2
				&& SafeAreaEdges.IsSoftInput(safeAreaView2.GetSafeAreaRegionsForEdge(3))) is not null;
		}


		/// <summary>
		/// Checks if the current measure information is still valid for the given constraints.
		/// This optimization avoids redundant measure operations when constraints haven't changed.
		/// </summary>
		/// <param name="widthConstraint">The width constraint to check</param>
		/// <param name="heightConstraint">The height constraint to check</param>
		/// <returns>True if the cached measure is still valid, false otherwise</returns>
		protected bool IsMeasureValid(double widthConstraint, double heightConstraint)
		{
			return widthConstraint == _lastMeasureWidth
				&& heightConstraint == _lastMeasureHeight;
		}

		/// <summary>
		/// Caches the measure constraints and the resulting measured size from the last measure operation.
		/// </summary>
		/// <param name="widthConstraint">The width constraint used.</param>
		/// <param name="heightConstraint">The height constraint used.</param>
		[Obsolete("Use CacheMeasureConstraints(double widthConstraint, double heightConstraint, Size measuredSize) instead.")]
		protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
			_lastMeasuredSize = null;
		}

		/// <summary>
		/// Caches the measure constraints and the resulting measured size from the last measure operation.
		/// </summary>
		/// <param name="widthConstraint">The width constraint used.</param>
		/// <param name="heightConstraint">The height constraint used.</param>
		/// <param name="measuredSize">The resulting measured cross-platform size.</param>
		protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint, Size measuredSize)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
			_lastMeasuredSize = measuredSize;
		}

		/// <summary>
		/// Determines if this view has been measured at least once.
		/// Used to decide whether a layout pass needs to perform measurement.
		/// </summary>
		/// <returns>True if the view has been measured, false otherwise</returns>
		bool HasBeenMeasured()
		{
			return _lastMeasuredSize.HasValue;
		}

		/// <summary>
		/// Invalidates the cached measure constraints, forcing a new measure operation
		/// on the next layout pass. This is called when the view's content or
		/// properties change in a way that affects its size.
		/// </summary>
		protected void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
			_lastMeasuredSize = null;
		}

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		/// <summary>
		/// Performs cross-platform measure operation, optionally adjusting for safe area.
		/// This method coordinates between the native iOS layout system and MAUI's
		/// cross-platform layout system.
		/// </summary>
		/// <param name="widthConstraint">The maximum width available</param>
		/// <param name="heightConstraint">The maximum height available</param>
		/// <returns>The desired size of the view</returns>
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
		/// Performs cross-platform arrange operation, optionally adjusting for safe area.
		/// This method positions and sizes child elements within the available bounds.
		/// </summary>
		/// <param name="bounds">The bounds rectangle to arrange within</param>
		void CrossPlatformArrange(CGRect bounds)
		{
			if (_appliesSafeAreaAdjustments)
			{
				bounds = AdjustForSafeArea(bounds);
			}

			CrossPlatformLayout?.CrossPlatformArrange(bounds.ToRectangle());
		}

		/// <summary>
		/// iOS method called to determine the size that fits within the given constraints.
		/// This integrates with the cross-platform layout system to provide consistent
		/// sizing behavior across platforms.
		/// </summary>
		/// <param name="size">The size constraints</param>
		/// <returns>The size that fits within the constraints</returns>
		public override CGSize SizeThatFits(CGSize size)
		{
			if (_crossPlatformLayoutReference == null)
			{
				return base.SizeThatFits(size);
			}

			var widthConstraint = (double)size.Width;
			var heightConstraint = (double)size.Height;

			if (IsMeasureValid(widthConstraint, heightConstraint) && _lastMeasuredSize is { } crossPlatformSize)
			{
				return crossPlatformSize.ToCGSize();
			}

			crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

			CacheMeasureConstraints(widthConstraint, heightConstraint, crossPlatformSize);

			return crossPlatformSize.ToCGSize();
		}

		/// <summary>
		/// iOS method called to layout subviews within this view's bounds.
		/// This method coordinates the cross-platform layout system with iOS layout,
		/// handling safe area adjustments and ensuring proper measure/arrange cycles.
		/// </summary>
		// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.LayoutVirtualView
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (_crossPlatformLayoutReference == null)
			{
				return;
			}

			// Validate safe area and check if it has changed in a way that affects layout
			if (!ValidateSafeArea())
			{
				InvalidateConstraintsCache();

				// The safe area is now interacting differently with our view, so we have to enqueue a second layout pass
				// to let ancestors adjust to the measured size.
				if (this.IsFinalMeasureHandledBySuperView())
				{
					//This arrangement step is essential for communicating the correct coordinate space to native iOS views before scheduling the second layout pass.
					//This ensures the native view is aware of the correct bounds and can adjust its layout accordingly.
					CrossPlatformArrange(Bounds.ToRectangle());
					SetNeedsLayout();
					this.InvalidateAncestorsMeasures();
					return;
				}
			}

			var bounds = Bounds.ToRectangle();

			var widthConstraint = bounds.Width;
			var heightConstraint = bounds.Height;

			// If the SuperView is a cross-platform layout backed view (i.e. MauiView, MauiScrollView, LayoutView, ..),
			// then measurement has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews.
			// This is especially important to avoid overriding potentially infinite measurement constraints
			// imposed by the parent (i.e. scroll view) with the current bounds, except when our bounds are fixed by constraints.
			// But we _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
			// the window); there's no guarantee that SizeThatFits has been called in that case.
			var needsMeasure = HasFixedConstraints switch
			{
				true => !HasBeenMeasured() || !this.IsFinalMeasureHandledBySuperView(),
				false => !IsMeasureValid(widthConstraint, heightConstraint) && !this.IsFinalMeasureHandledBySuperView()
			};

			if (needsMeasure)
			{
				var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasureConstraints(widthConstraint, heightConstraint, crossPlatformSize);
			}

			CrossPlatformArrange(bounds);
		}

		/// <summary>
		/// Validates and updates the safe area state for this view.
		/// This method checks if the safe area has changed and updates the internal state accordingly.
		/// </summary>
		/// <returns>True if the safe area interaction hasn't changed, false if it has changed and requires layout updates</returns>
		bool ValidateSafeArea()
		{
			UpdateKeyboardSubscription();

			// If nothing changed, we don't need to do anything
			if (!_safeAreaInvalidated)
			{
				return true;
			}

			// Mark the safe area as validated given that we're about to check it
			_safeAreaInvalidated = false;

			// Store the information about the safe area for developers to use
			if (View is ISafeAreaView2 safeAreaPage)
			{
				safeAreaPage.SafeAreaInsets = SafeAreaInsets.ToThickness();
			}

			var oldSafeArea = _safeArea;
			_safeArea = GetAdjustedSafeAreaInsets();

			var oldApplyingSafeAreaAdjustments = _appliesSafeAreaAdjustments;
			_appliesSafeAreaAdjustments = RespondsToSafeArea() && !_safeArea.IsEmpty;

			// Return whether the way safe area interacts with our view has changed
			return oldApplyingSafeAreaAdjustments == _appliesSafeAreaAdjustments &&
				   (oldSafeArea == _safeArea || !_appliesSafeAreaAdjustments);
		}

		/// <summary>
		/// Provides the visual tree element that this platform view represents.
		/// This is used by the visual tree system to navigate between platform and cross-platform views.
		/// </summary>
		/// <returns>The IVisualTreeElement this view represents, or null if none</returns>
		IVisualTreeElement? IVisualTreeElementProvidable.GetElement()
		{

			if (View is IVisualTreeElement viewElement &&
				viewElement.IsThisMyPlatformView(this))
			{
				return viewElement;
			}

			if (CrossPlatformLayout is IVisualTreeElement layoutElement &&
				layoutElement.IsThisMyPlatformView(this))
			{
				return layoutElement;
			}

			return null;
		}

		/// <summary>
		/// Marks this view to invalidate ancestor measures when moved to a window.
		/// This is used when the view's constraints or size requirements change in a way
		/// that affects the entire view hierarchy.
		/// </summary>
		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		/// <summary>
		/// Invalidates the measure for this view and determines if the invalidation should propagate to ancestors.
		/// This is part of the measure invalidation system that ensures layout updates when view properties change.
		/// </summary>
		/// <param name="isPropagating">True if this invalidation is propagating from a descendant view</param>
		/// <returns>True if the invalidation should continue propagating to ancestors, false to stop propagation</returns>
		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			_safeAreaInvalidated = true;
			InvalidateConstraintsCache();
			SetNeedsLayout();

			// If we're propagating, we can stop at the first view with fixed constraints
			if (isPropagating && HasFixedConstraints)
			{
				// We're stopping propagation here, but we have to account for the wrapper view
				// which needs to be invalidated for consistency too.
				if (Superview is WrapperView wrapper)
				{
					wrapper.SetNeedsLayout();
				}

				return false;
			}

			// If we're not propagating, then this view is the one triggering the invalidation
			// and one possible cause is that constraints have changed, so we have to propagate the invalidation.
			return true;
		}

		/// <summary>
		/// Event handler for the MovedToWindow event. This field is used to store
		/// subscriptions to the IUIViewLifeCycleEvents.MovedToWindow event.
		/// </summary>
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;

		/// <summary>
		/// Event fired when this view is moved to a window (added to or removed from the view hierarchy).
		/// This is part of the IUIViewLifeCycleEvents interface and allows subscribers to react to
		/// view lifecycle changes.
		/// </summary>
		event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}


		public override void SafeAreaInsetsDidChange()
		{
			_safeAreaInvalidated = true;
			base.SafeAreaInsetsDidChange();
		}

		/// <summary>
		/// Called when this view is moved to a window (added to or removed from the view hierarchy).
		/// This triggers safe area invalidation and any pending ancestor measure invalidations.
		/// </summary>
		public override void MovedToWindow()
		{
			base.MovedToWindow();

			_scrollViewDescendant = null;

			// Notify any subscribers that this view has been moved to a window
			_movedToWindow?.Invoke(this, EventArgs.Empty);

			// Safe area may have changed when moving to a different window
			_safeAreaInvalidated = true;

			// If we were marked to invalidate ancestors when moved to window, do so now
			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}

			UpdateKeyboardSubscription();
		}

		/// <summary>
		/// Gets a value indicating whether this view can become the first responder.
		/// Always returns <c>true</c>, allowing the view to receive input events.
		/// </summary>
		public override bool CanBecomeFirstResponder => true;

		/// <summary>
		/// Called when the focus environment updates. This method propagates native iOS focus
		/// changes to the cross-platform layer by updating the IsFocused property of the
		/// associated IView when this MauiView gains or loses focus.
		/// </summary>
		/// <param name="context">Information about the focus update</param>
		/// <param name="coordinator">Coordinator for focus animations</param>
		public override void DidUpdateFocus(UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator)
		{
			base.DidUpdateFocus(context, coordinator);

			var wasFocused = _isFocused;
			var isFocused = context.NextFocusedView == this;

			// Only update if focus state actually changed to avoid unnecessary work
			if (wasFocused != isFocused)
			{
				_isFocused = isFocused;
				UpdateCrossPlatformFocusState(isFocused);
			}
		}

		/// <summary>
		/// Updates the cross-platform layer's focus state
		/// </summary>
		/// <param name="isFocused">The new focus state</param>
		void UpdateCrossPlatformFocusState(bool isFocused)
		{
			if (CrossPlatformLayout is IView view)
			{
				// Update the cross-platform focus state
				view.IsFocused = isFocused;

				if (isFocused)
				{
					view.Focus();
				}
				else
				{
					view.Unfocus();
				}
			}
		}

		public override bool BecomeFirstResponder()
		{
			var result = base.BecomeFirstResponder();
			if (result && !_isFocused)
			{
				_isFocused = true;
				UpdateCrossPlatformFocusState(true);
			}
			return result;
		}

		public override bool ResignFirstResponder()
		{
			var result = base.ResignFirstResponder();
			if (result && _isFocused)
			{
				_isFocused = false;
				UpdateCrossPlatformFocusState(false);
			}
			return result;
		}
	}
}