using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
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
		

		WeakReference<IView>? _reference;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		public IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		bool HasFixedConstraints => CrossPlatformLayout is IConstrainedView { HasFixedConstraints: true };

		bool RespondsToSafeArea()
		{
			// Check if we should respond to safe area on any edge
			if (View is ISafeAreaPage safeAreaPage)
			{
				// If any edge should respect safe area, then we respond to safe area
				for (int edge = 0; edge < 4; edge++)
				{
					if (!safeAreaPage.IgnoreSafeAreaForEdge(edge))
					{
						// At least one edge should respect safe area
						if (_respondsToSafeArea.HasValue)
						{
							return _respondsToSafeArea.Value;
						}
						return (_respondsToSafeArea = RespondsToSelector(new Selector("safeAreaInsets"))).Value;
					}
				}
				// All edges ignore safe area
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
			if (KeyboardAutoManagerScroll.ShouldIgnoreSafeAreaAdjustment)
			{
				KeyboardAutoManagerScroll.ShouldScrollAgain = true;
			}

			// Create a selective safe area based on per-edge settings
			var selectiveSafeArea = GetSelectiveSafeArea();
			return selectiveSafeArea.InsetRect(bounds);
		}

		SafeAreaPadding GetSelectiveSafeArea()
		{
			if (View is ISafeAreaPage safeAreaPage)
			{
				// Apply safe area selectively per edge
				var left = safeAreaPage.IgnoreSafeAreaForEdge(0) ? 0 : _safeArea.Left;
				var top = safeAreaPage.IgnoreSafeAreaForEdge(1) ? 0 : _safeArea.Top;
				var right = safeAreaPage.IgnoreSafeAreaForEdge(2) ? 0 : _safeArea.Right;
				var bottom = safeAreaPage.IgnoreSafeAreaForEdge(3) ? 0 : _safeArea.Bottom;

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
			if (!_safeAreaInvalidated)
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
			_safeArea = SafeAreaInsets.ToSafeAreaInsets();

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
		}
	}
}