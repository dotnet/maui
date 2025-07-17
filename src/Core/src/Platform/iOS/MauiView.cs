using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public abstract class MauiView : UIView, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, IUIViewLifeCycleEvents, IPlatformMeasureInvalidationController
	{
		static bool? _respondsToSafeArea;

		bool _invalidateParentWhenMovedToWindow;

		double _lastMeasureHeight = double.NaN;
		double _lastMeasureWidth = double.NaN;

		SafeAreaPadding _safeArea = SafeAreaPadding.Empty;
		bool _safeAreaInvalidated = true;
		bool _appliesSafeAreaAdjustments;
		
		// Keyboard tracking
		CGRect _keyboardFrame = CGRect.Empty;
		bool _isKeyboardShowing;
		WeakReference<NSObject>? _keyboardWillShowObserver;
		WeakReference<NSObject>? _keyboardWillHideObserver;

		WeakReference<IView>? _reference;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		public IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		bool HasFixedConstraints => CrossPlatformLayout is IConstrainedView { HasFixedConstraints: true };

		SafeAreaRegions GetSafeAreaRegionForEdge(int edge)
		{
			if (View is ISafeAreaPage safeAreaPage)
			{
				return safeAreaPage.GetSafeAreaRegionsForEdge(edge);
			}
			
			// Fallback to legacy ISafeAreaView behavior
			if (View is ISafeAreaView sav)
			{
				return sav.IgnoreSafeArea ? SafeAreaRegions.All : SafeAreaRegions.Default;
			}
			
			return SafeAreaRegions.Default; // Default: respect safe area
		}

		bool ShouldIgnoreSafeAreaForEdge(int edge)
		{
			if (View is ISafeAreaPage safeAreaPage)
			{
				return safeAreaPage.IgnoreSafeAreaForEdge(edge);
			}
			
			// Fallback to legacy ISafeAreaView behavior
			if (View is ISafeAreaView sav)
			{
				return sav.IgnoreSafeArea;
			}
			
			return false; // Default: respect safe area
		}

		static double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea)
		{
			return safeAreaRegion switch
			{
				SafeAreaRegions.All => 0, // Ignore all insets - content may be positioned anywhere
				SafeAreaRegions.None => originalSafeArea, // Never display behind blocking UI - always respect safe area
				SafeAreaRegions.SoftInput => originalSafeArea, // Layout behind soft input - for now, respect safe area (can be enhanced later)
				SafeAreaRegions.Default => originalSafeArea, // Apply platform insets - respect safe area
				_ => originalSafeArea // Default case - respect safe area
			};
		}

		bool RespondsToSafeArea()
		{
			// Check if we should respond to safe area on any edge
			if (View is ISafeAreaPage safeAreaPage)
			{
				// If any edge should respect safe area, then we respond to safe area
				for (int edge = 0; edge < 4; edge++)
				{
					var safeAreaRegion = safeAreaPage.GetSafeAreaRegionsForEdge(edge);
					// If this edge doesn't ignore all insets, we need to respond to safe area
					if (safeAreaRegion != SafeAreaRegions.All)
					{
						// At least one edge should respect safe area
						if (_respondsToSafeArea.HasValue)
						{
							return _respondsToSafeArea.Value;
						}

						return (_respondsToSafeArea = RespondsToSelector(new Selector("safeAreaInsets"))).Value;
					}
				}
				// All edges ignore safe area (All = ignore all insets)
				return false;
			}

			// Fallback to legacy ISafeAreaView behavior
			if (View is not ISafeAreaView sav || sav.IgnoreSafeArea)
			{
				return false;
			}

			if (_respondsToSafeArea.HasValue)
			{
				return _respondsToSafeArea.Value;
			}

			return (_respondsToSafeArea = RespondsToSelector(new Selector("safeAreaInsets"))).Value;
		}

		protected CGRect AdjustForSafeArea(CGRect bounds)
		{
			if (KeyboardAutoManagerScroll.ShouldIgnoreSafeAreaAdjustment)
			{
				KeyboardAutoManagerScroll.ShouldScrollAgain = true;
			}

			return _safeArea.InsetRect(bounds);
		}

		protected CGRect AdjustForSafeAreaPerEdge(CGRect bounds)
		{
			// Create a selective safe area based on per-edge settings
			var selectiveSafeArea = GetSelectiveSafeArea();
			return selectiveSafeArea.InsetRect(bounds);
		}

		void SubscribeToKeyboardNotifications()
		{
			UnsubscribeFromKeyboardNotifications();

			var showObserver = NSNotificationCenter.DefaultCenter.AddObserver(
				UIKeyboard.WillShowNotification,
				OnKeyboardWillShow);
			_keyboardWillShowObserver = new WeakReference<NSObject>(showObserver);

			var hideObserver = NSNotificationCenter.DefaultCenter.AddObserver(
				UIKeyboard.WillHideNotification,
				OnKeyboardWillHide);
			_keyboardWillHideObserver = new WeakReference<NSObject>(hideObserver);

			// TODO Temporary
			UnsubscribeFromKeyboardNotifications();
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

		void OnKeyboardWillShow(NSNotification notification)
		{
			var keyboardFrame = GetKeyboardFrame(notification);
			if (keyboardFrame.HasValue)
			{
				_keyboardFrame = keyboardFrame.Value;
				_isKeyboardShowing = true;
				_safeAreaInvalidated = true;
				SetNeedsLayout();
			}
		}

		void OnKeyboardWillHide(NSNotification notification)
		{
			_keyboardFrame = CGRect.Empty;
			_isKeyboardShowing = false;
			_safeAreaInvalidated = true;
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
			// Todo disable keyboard for now
			_isKeyboardShowing = false;
			var baseSafeArea = SafeAreaInsets.ToSafeAreaInsets();

			// Check if keyboard-aware safe area adjustments are needed
			if (View is ISafeAreaPage safeAreaPage && _isKeyboardShowing)
			{
				// Check if any edge has SafeAreaRegions.SoftInput or SafeAreaRegions.None set
				var needsKeyboardAdjustment = false;
				for (int edge = 0; edge < 4; edge++)
				{
					var safeAreaRegion = safeAreaPage.GetSafeAreaRegionsForEdge(edge);
					if (safeAreaRegion == SafeAreaRegions.SoftInput || safeAreaRegion == SafeAreaRegions.None)
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
							// Calculate keyboard height in the window's coordinate system
							var keyboardHeight = keyboardIntersection.Height;
							
							// For SafeAreaRegions.SoftInput: Layout behind soft input down to where soft input starts
							// For SafeAreaRegions.None: Never display behind blocking UI (most conservative)
							// Both cases need to ensure content doesn't go behind the keyboard
							
							// Bottom edge is most commonly affected by keyboard
							var bottomEdgeRegion = safeAreaPage.GetSafeAreaRegionsForEdge(3); // 3 = bottom edge
							if (bottomEdgeRegion == SafeAreaRegions.SoftInput || bottomEdgeRegion == SafeAreaRegions.None)
							{
								// Use the larger of the current bottom safe area or the keyboard height
								var adjustedBottom = Math.Max(baseSafeArea.Bottom, keyboardHeight);
								baseSafeArea = new SafeAreaPadding(baseSafeArea.Left, baseSafeArea.Right, baseSafeArea.Top, adjustedBottom);
							}
						}
					}
				}
			}

			return baseSafeArea;
		}

		SafeAreaPadding GetSelectiveSafeArea()
		{
			if (View is ISafeAreaPage safeAreaPage)
			{
				// Apply safe area selectively per edge based on SafeAreaRegions
				var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0), _safeArea.Left);
				var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1), _safeArea.Top);
				var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2), _safeArea.Right);
				var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3), _safeArea.Bottom);

				return new SafeAreaPadding(left, right, top, bottom);
			}

			// Fallback to legacy behavior
			if (View is ISafeAreaView sav && sav.IgnoreSafeArea)
			{
				return SafeAreaPadding.Empty;
			}

			return _safeArea;
		}

		protected bool IsMeasureValid(double widthConstraint, double heightConstraint)
		{
			return !HasFixedConstraints
				&& widthConstraint == _lastMeasureWidth
				&& heightConstraint == _lastMeasureHeight;
		}

		protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
		}

		protected void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
		}

		public override void SafeAreaInsetsDidChange()
		{
			base.SafeAreaInsetsDidChange();

			// We can't do anything more than this here because `SafeAreaInsetsDidChange()` is triggered twice with
			// different values when setting the frame via `Center` and `Bounds` in `PlatformArrangeHandler`.
			_safeAreaInvalidated = true;
		}

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			if (_appliesSafeAreaAdjustments)
			{
				// When responding to safe area, we need to adjust the constraints to account for the selective safe area.
				var selectiveSafeArea = GetSelectiveSafeArea();
				widthConstraint -= selectiveSafeArea.HorizontalThickness;
				heightConstraint -= selectiveSafeArea.VerticalThickness;
			}

			var crossPlatformSize = CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;

			if (_appliesSafeAreaAdjustments)
			{
				// If we're responding to the safe area, we need to add the safe area back to the size so the container can allocate the correct space
				var selectiveSafeArea = GetSelectiveSafeArea();
				crossPlatformSize = new Size(crossPlatformSize.Width + selectiveSafeArea.HorizontalThickness, crossPlatformSize.Height + selectiveSafeArea.VerticalThickness);
			}

			return crossPlatformSize;
		}

		void CrossPlatformArrange(CGRect bounds)
		{
			if (_appliesSafeAreaAdjustments)
			{
				bounds = AdjustForSafeAreaPerEdge(bounds);
			}

			CrossPlatformLayout?.CrossPlatformArrange(bounds.ToRectangle());
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (_crossPlatformLayoutReference == null)
			{
				return base.SizeThatFits(size);
			}

			var widthConstraint = (double)size.Width;
			var heightConstraint = (double)size.Height;

			var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

			CacheMeasureConstraints(widthConstraint, heightConstraint);

			return crossPlatformSize.ToCGSize();
		}

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

			// TODO: This is expensive and should not be happening during arrange
			// This happens on the first layout pass when the default size of the page is set
			// If we're here and the fixed constraints have changed, it's likely that the content was invalidated 
			// and we need to remeasure
			if (!IsMeasureValid(widthConstraint, heightConstraint))
			{
				var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasureConstraints(widthConstraint, heightConstraint);
			}

			CrossPlatformArrange(bounds);
		}

		bool ValidateSafeArea()
		{
			// If nothing changed, we don't need to do anything
			if (!_safeAreaInvalidated && GetAdjustedSafeAreaInsets() != _safeArea)
			{
				return true;
			}

			// Mark the safe area as validated given that we're about to check it
			_safeAreaInvalidated = false;

			// Store the information about the safe area for developers to use
			if (View is ISafeAreaPage safeAreaPage)
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

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
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

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();

			_movedToWindow?.Invoke(this, EventArgs.Empty);
			_safeAreaInvalidated = true;

			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}

			// Subscribe to keyboard notifications when moved to window
			if (Window != null)
			{
				SubscribeToKeyboardNotifications();
			}
			else
			{
				UnsubscribeFromKeyboardNotifications();
			}
		}
	}
}