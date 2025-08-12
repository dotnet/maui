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
			pageControl.InvalidateMeasure();
		}

		public static void UpdateHideSingle(this UIPageControl pageControl, IIndicatorView indicatorView)
			=> pageControl.HidesForSinglePage = indicatorView.HideSingle;

		public static void UpdateCurrentPage(this UIPageControl pageControl, int currentPage)
			=> pageControl.CurrentPage = currentPage;

		public static void UpdatePages(this UIPageControl pageControl, int pageCount)
			=> pageControl.Pages = pageCount;

		public static void UpdatePagesIndicatorTintColor(this UIPageControl pageControl, IIndicatorView indicatorView)
			=> pageControl.PageIndicatorTintColor = indicatorView.IndicatorColor?.ToColor()?.ToPlatform();

		public static void UpdateCurrentPagesIndicatorTintColor(this UIPageControl pageControl, IIndicatorView indicatorView)
			=> pageControl.CurrentPageIndicatorTintColor = indicatorView.SelectedIndicatorColor?.ToColor()?.ToPlatform();
	}
}
