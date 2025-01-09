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
		static bool? _respondsToSafeArea;

		double _lastMeasureHeight = double.NaN;
		double _lastMeasureWidth = double.NaN;

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

			if (_respondsToSafeArea.HasValue)
				return _respondsToSafeArea.Value;

			return (bool)(_respondsToSafeArea = RespondsToSelector(new Selector("safeAreaInsets")));

		}

		protected CGRect AdjustForSafeArea(CGRect bounds)
		{
			if (KeyboardAutoManagerScroll.ShouldIgnoreSafeAreaAdjustment)
			{
				KeyboardAutoManagerScroll.ShouldScrollAgain = true;
			}

			if (!RespondsToSafeArea())
			{
				return bounds;
			}

#pragma warning disable CA1416 // TODO 'UIView.SafeAreaInsets' is only supported on: 'ios' 11.0 and later, 'maccatalyst' 11.0 and later, 'tvos' 11.0 and later.
			return SafeAreaInsets.InsetRect(bounds);
#pragma warning restore CA1416
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
				isav2.SafeAreaInsets = this.SafeAreaInsets.ToThickness();
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

		// SizeThatFits does not take into account the constraints set on the view.
		// For example, if the user has set a width and height on this view, those constraints
		// will not be reflected in the value returned from this method. This method purely returns
		// a measure based on the size that is passed in.
		// The constraints are all applied by ViewHandlerExtensions.GetDesiredSizeFromHandler
		// after it calls this method.
		public override CGSize SizeThatFits(CGSize size)
		{
			if (_crossPlatformLayoutReference == null)
			{
				return base.SizeThatFits(size);
			}

			var widthConstraint = size.Width;
			var heightConstraint = size.Height;

			var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

			CacheMeasureConstraints(widthConstraint, heightConstraint);

			// If for some reason the upstream measure passes in a negative contraint
			// Lets just bypass this code
			if (RespondsToSafeArea() && widthConstraint >= 0 && heightConstraint >= 0)
			{
				// During the LayoutSubViews pass, we adjust the Bounds of this view for the safe area and then pass the adjusted result to CrossPlatformArrange.
				// The CrossPlatformMeasure call does not include the safe area, so we need to add it here to ensure the returned size is correct.
				//
				// For example, if this is a layout with an Entry of height 20, CrossPlatformMeasure will return a height of 20.
				// This means the bounds will be set to a height of 20, causing AdjustForSafeArea(Bounds) to return a negative bounds once it has
				// subtracted the safe area insets. Therefore, we need to add the safe area insets to the CrossPlatformMeasure result to ensure correct arrangement.
				var widthSafeAreaOffset = SafeAreaInsets.Left + SafeAreaInsets.Right;
				var heightSafeAreaOffset = SafeAreaInsets.Top + SafeAreaInsets.Bottom;

				var width = double.Clamp(crossPlatformSize.Width + widthSafeAreaOffset, 0, widthConstraint);
				var height = double.Clamp(crossPlatformSize.Height + heightSafeAreaOffset, 0, heightConstraint);

				return new CGSize(width, height);
			}

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

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();

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
