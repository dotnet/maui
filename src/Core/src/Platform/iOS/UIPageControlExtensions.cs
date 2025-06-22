using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class UIPageControlExtensions
	{
		public static void UpdateIndicatorShape(this MauiPageControl pageControl, IIndicatorView indicatorView)
		{
			pageControl.IsSquare = !indicatorView.IsCircleShape();

			pageControl.LayoutSubviews();
		}

		public static void UpdateIndicatorSize(this MauiPageControl pageControl, IIndicatorView indicatorView)
		{
			pageControl.IndicatorSize = indicatorView.IndicatorSize;
			pageControl.LayoutSubviews();
		}

		public static void UpdateHideSingle(this UIPageControl pageControl, IIndicatorView indicatorView)
			=> pageControl.HidesForSinglePage = indicatorView.HideSingle;

		public static void UpdateCurrentPage(this UIPageControl pageControl, int currentPage)
			=> pageControl.CurrentPage = currentPage;

		public static void UpdatePages(this UIPageControl pageControl, int pageCount)
		{
			// UIKit does not always refresh the visible dots when updating the `Pages` property directly.
			// To force a proper layout and visual update, temporarily remove and re-add the control to its superview.

			var superview = pageControl.Superview;
			if (superview is null)
			{
				// If the control is not in a view hierarchy, just set the value directly.
				pageControl.Pages = pageCount;
				return;
			}

			// Find the index of the page control in its superview’s subview list
			var index = Array.IndexOf(superview.Subviews, pageControl);

			// Remove and immediately reinsert the control to force UIKit to reset its internal state.
			pageControl.RemoveFromSuperview();
			superview.InsertSubview(pageControl, index);

			// Now set the actual page count — UIKit will correctly render the new number of indicators.
			pageControl.Pages = pageCount;
		}

		public static void UpdatePagesIndicatorTintColor(this UIPageControl pageControl, IIndicatorView indicatorView)
			=> pageControl.PageIndicatorTintColor = indicatorView.IndicatorColor?.ToColor()?.ToPlatform();

		public static void UpdateCurrentPagesIndicatorTintColor(this UIPageControl pageControl, IIndicatorView indicatorView)
			=> pageControl.CurrentPageIndicatorTintColor = indicatorView.SelectedIndicatorColor?.ToColor()?.ToPlatform();
	}
}
