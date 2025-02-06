using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, ICrossPlatformLayoutBacking
	{
		bool _invalidateParentWhenMovedToWindow;
		double _lastMeasureHeight;
		double _lastMeasureWidth;
		double _lastArrangeHeight;
		double _lastArrangeWidth;

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
			if (frameChanged && CrossPlatformLayout is { } crossPlatformLayout)
			{
				_lastArrangeWidth = widthConstraint;
				_lastArrangeHeight = heightConstraint;

				// If the SuperView is a cross-platform layout backed view (i.e. MauiView, MauiScrollView, LayoutView, ..),
				// then measurement has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews.
				// This is especially important to avoid overriding potentially infinite measurement constraints
				// imposed by the parent (i.e. scroll view) with the current bounds.
				// But we _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
				// the window); there's no guarantee that SizeThatFits has been called in that case.
				if (!IsMeasureValid(widthConstraint, heightConstraint) && !this.IsFinalMeasureHandledBySuperView())
				{
					crossPlatformLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);
					CacheMeasureConstraints(widthConstraint, heightConstraint);
				}

				// Account for safe area adjustments automatically added by iOS
				var crossPlatformBounds = AdjustedContentInset.InsetRect(bounds).Size.ToSize();
				var size = crossPlatformLayout.CrossPlatformArrange(new Rect(new Point(), crossPlatformBounds));
				ContentSize = size.ToCGSize();
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

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			InvalidateConstraintsCache();

			TryToInvalidateSuperView(false);
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

		private protected void TryToInvalidateSuperView(bool shouldOnlyInvalidateIfPending)
		{
			if (shouldOnlyInvalidateIfPending && !_invalidateParentWhenMovedToWindow)
			{
				return;
			}

			// We check for Window to avoid scenarios where an invalidate might propagate up the tree
			// To a SuperView that's been disposed which will cause a crash when trying to access it
			if (Window is not null)
			{
				this.Superview?.SetNeedsLayout();
				_invalidateParentWhenMovedToWindow = false;
			}
			else
			{
				_invalidateParentWhenMovedToWindow = true;
			}
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
			TryToInvalidateSuperView(true);
		}
	}
}