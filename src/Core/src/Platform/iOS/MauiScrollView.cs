using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents
	{
		bool _arranged;
		bool _invalidateParentWhenMovedToWindow;
		double _lastMeasureHeight;
		double _lastMeasureWidth;

		WeakReference<IScrollView>? _reference;

		internal IScrollView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		public override void LayoutSubviews()
		{
			// LayoutSubviews is invoked while scrolling, so we need to arrange the content only when it's necessary.
			// This could be done via `override ScrollViewHandler.PlatformArrange` but that wouldn't cover the case
			// when the ScrollView is attached to a non-MauiView parent (i.e. DeviceTests).
			if (!_arranged && View is { } scrollView)
			{
				var bounds = Bounds;
				var widthConstraint = (double)bounds.Width;
				var heightConstraint = (double)bounds.Height;

				// If the SuperView is a MauiView (backing a cross-platform ContentView or Layout), then measurement
				// has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews. But we
				// _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
				// the window); there's no guarantee that SizeThatFits has been called in that case.
				if (!IsMeasureValid(widthConstraint, heightConstraint) && Superview is not MauiView)
				{
					CrossPlatformMeasure(scrollView, widthConstraint, heightConstraint);
					CacheMeasureConstraints(widthConstraint, heightConstraint);
				}

				// Account for safe area adjustments automatically added by iOS
				var crossPlatformBounds = AdjustedContentInset.InsetRect(bounds).Size.ToSize();
				var size = scrollView.ArrangeContentUnbounded(new Rect(new Point(), crossPlatformBounds));
				ContentSize = size.ToCGSize();
				_arranged = true;
			}

			base.LayoutSubviews();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (View is not { } scrollView)
			{
				return new CGSize();
			}

			var widthConstraint = (double)size.Width;
			var heightConstraint = (double)size.Height;

			var contentSize = CrossPlatformMeasure(scrollView, widthConstraint, heightConstraint);
			CacheMeasureConstraints(widthConstraint, heightConstraint);

			return contentSize;
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			InvalidateConstraintsCache();
			_arranged = false;

			TryToInvalidateSuperView(false);
		}

		static Size CrossPlatformMeasure(IScrollView scrollView, double widthConstraint, double heightConstraint)
		{
			var scrollOrientation = scrollView.Orientation;
			var contentWidthConstraint = scrollOrientation is ScrollOrientation.Horizontal or ScrollOrientation.Both ? double.PositiveInfinity : widthConstraint;
			var contentHeightConstraint = scrollOrientation is ScrollOrientation.Vertical or ScrollOrientation.Both ? double.PositiveInfinity : heightConstraint;
			var contentSize = scrollView.MeasureContent(scrollView.Padding, contentWidthConstraint, contentHeightConstraint);

			// Our target size is the smaller of it and the constraints
			var width = contentSize.Width <= widthConstraint ? contentSize.Width : widthConstraint;
			var height = contentSize.Height <= heightConstraint ? contentSize.Height : heightConstraint;

			width = ViewHandlerExtensions.ResolveConstraints(width, scrollView.Width, scrollView.MinimumWidth, scrollView.MaximumWidth);
			height = ViewHandlerExtensions.ResolveConstraints(height, scrollView.Height, scrollView.MinimumHeight, scrollView.MaximumHeight);

			return new Size(width, height);
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