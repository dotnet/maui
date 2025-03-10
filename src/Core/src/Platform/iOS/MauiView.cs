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
		bool _invalidateParentWhenMovedToWindow;
		bool? _insideScrollView;
		static bool? _respondsToSafeArea;

		double _lastMeasureHeight = double.NaN;
		double _lastMeasureWidth = double.NaN;
		Thickness _lastSafeArea;

		WeakReference<IView>? _reference;
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

		public IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		bool RespondsToSafeArea()
		{
			if (View is not ISafeAreaView sav || sav.IgnoreSafeArea)
			{
				return false;
			}

			if (Window is not null)
			{
				// If I'm inside a scrollable container then I don't need to adjust for the safe area
				// The scroll view will handle insetting the content if needed
				_insideScrollView ??= this.GetParentOfType<UIScrollView>() is not null;				
				if (_insideScrollView.Value)
				{
					return false;
				}
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

			if (_lastSafeArea.IsEmpty)
			{
				return bounds;
			}

			return new CGRect(
				bounds.Left + _lastSafeArea.Left,
				bounds.Top + _lastSafeArea.Top,
				bounds.Width - _lastSafeArea.HorizontalThickness,
				bounds.Height - _lastSafeArea.VerticalThickness);
		}

		protected bool IsMeasureValid(double widthConstraint, double heightConstraint)
		{
			// Check the last constraints this View was measured with; if they're the same,
			// then the current measure info is already correct and we don't need to repeat it
			return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
		}

		protected void InvalidateConstraintsCache()
		{
			_lastMeasureWidth = double.NaN;
			_lastMeasureHeight = double.NaN;
		}

		protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
		{
			_lastMeasureWidth = widthConstraint;
			_lastMeasureHeight = heightConstraint;
		}

		public override void SafeAreaInsetsDidChange()
		{
			base.SafeAreaInsetsDidChange();

			if (View is ISafeAreaView2 isav2)
			{
				isav2.SafeAreaInsets = this.SafeAreaInsets.ToThickness();
			}

			if (RespondsToSafeArea() && _lastSafeArea != SafeAreaInsets.ToThickness())
			{
				// The safe area is now interacting differently with our view, so we need to remeasure and rearrange
				// give the constraints are now different.
				this.InvalidateAncestorsMeasures();
			}
		}

		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

		Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		Size CrossPlatformArrange(Rect bounds)
		{
			return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (_crossPlatformLayoutReference == null)
			{
				return base.SizeThatFits(size);
			}

			var widthConstraint = size.Width;
			var heightConstraint = size.Height;

			var respondsToSafeArea = RespondsToSafeArea();
			if (respondsToSafeArea)
			{
				// When responding to safe area, we need to adjust the constraints to account for the safe area.
				// We store the safe area measurements so we can adjust the bounds properly when arranging.
				_lastSafeArea = SafeAreaInsets.ToThickness();

				widthConstraint -= (float)_lastSafeArea.HorizontalThickness;
				heightConstraint -= (float)_lastSafeArea.VerticalThickness;
			}

			var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

			if (respondsToSafeArea)
			{
				// If we're responding to the safe area, we need to add the safe area back to the size so the container can allocate the correct space
				crossPlatformSize = new Size(crossPlatformSize.Width + _lastSafeArea.HorizontalThickness, crossPlatformSize.Height + _lastSafeArea.VerticalThickness);
			}

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

			var bounds = Bounds.ToRectangle();

			var widthConstraint = bounds.Width;
			var heightConstraint = bounds.Height;

			// If the SuperView is a cross-platform layout backed view (i.e. MauiView, MauiScrollView, LayoutView, ..),
			// then measurement has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews.
			// This is especially important to avoid overriding potentially infinite measurement constraints
			// imposed by the parent (i.e. scroll view) with the current bounds.
			// But we _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
			// the window); there's no guarantee that SizeThatFits has been called in that case.
			if (!IsMeasureValid(widthConstraint, heightConstraint) && !this.IsFinalMeasureHandledBySuperView())
			{
				CrossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasureConstraints(widthConstraint, heightConstraint);
			}

			bounds = AdjustForSafeArea(Bounds).ToRectangle();
			CrossPlatformArrange(bounds);
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

		void IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			InvalidateConstraintsCache();
			SetNeedsLayout();
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
			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}
	}
}
