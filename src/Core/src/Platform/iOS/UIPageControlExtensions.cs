using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform.iOS
{
	public static class UIPageControlExtensions
	{
		public static void UpdateIndicatorShape(this MauiPageControl pageControl, IIndicatorView indicatorView)
		{
			bool IsCircleShape()
			{
				var sH = indicatorView.IndicatorsShape.Shape;
				var pointsCount = 13;
				if (sH != null)
				{
					var path = sH.PathForBounds(new Rectangle(0, 0, 6, 6));
					pointsCount = path.Count;
				}

				return pointsCount == 13;
			}
			pageControl.IsSquare = !IsCircleShape();
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
			=> pageControl.Pages = pageCount;

		public static void UpdatePagesIndicatorTintColor(this UIPageControl pageControl, IIndicatorView indicatorView)
			=> pageControl.PageIndicatorTintColor = indicatorView.IndicatorColor?.ToColor()?.ToNative();

		public static void UpdateCurrentPagesIndicatorTintColor(this UIPageControl pageControl, IIndicatorView indicatorView)
			=> pageControl.CurrentPageIndicatorTintColor = indicatorView.SelectedIndicatorColor?.ToColor()?.ToNative();
	}
}
