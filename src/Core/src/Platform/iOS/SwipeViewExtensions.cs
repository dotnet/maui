using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static partial class SwipeViewExtensions
	{
		internal static Size GetSwipeItemSize(this ISwipeView swipeView, ISwipeItem swipeItem, UIView contentView, SwipeDirection? swipeDirection)
		{
			var items = GetSwipeItemsByDirection(swipeView, swipeDirection);
			double threshold = swipeView.Threshold;
			if (items == null)
				return Size.Zero;

			var contentHeight = contentView.Frame.Height;
			var contentWidth = contentView.Frame.Width;

			if (swipeDirection.IsHorizontalSwipe())
			{
				if (swipeItem is ISwipeItemMenuItem)
				{
					return new Size(items.Mode == SwipeMode.Execute ? (threshold > 0 ? threshold : contentWidth) / items.Count : (threshold < SwipeItemWidth ? SwipeItemWidth : threshold), contentHeight);
				}

				if (swipeItem is ISwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);

					double swipeItemWidth;

					if (swipeItemViewSizeRequest.Width > 0)
						swipeItemWidth = threshold > swipeItemViewSizeRequest.Width ? threshold : swipeItemViewSizeRequest.Width;
					else
						swipeItemWidth = threshold > SwipeItemWidth ? threshold : SwipeItemWidth;

					return new Size(swipeItemWidth, contentHeight);
				}
			}
			else
			{
				if (swipeItem is ISwipeItemMenuItem)
				{
					var swipeItemHeight = GetSwipeItemHeight(swipeView, swipeDirection, contentView);
					return new Size(contentWidth / items.Count, (threshold > 0 && threshold < swipeItemHeight) ? threshold : swipeItemHeight);
				}

				if (swipeItem is ISwipeItemView verticalSwipeItemView)
				{
					var swipeItemViewSizeRequest = verticalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);

					double swipeItemHeight;

					if (swipeItemViewSizeRequest.Width > 0)
						swipeItemHeight = threshold > swipeItemViewSizeRequest.Height ? threshold : (float)swipeItemViewSizeRequest.Height;
					else
						swipeItemHeight = threshold > contentHeight ? threshold : contentHeight;

					return new Size(contentWidth / items.Count, swipeItemHeight);
				}
			}

			return Size.Zero;
		}

		internal static double GetSwipeItemHeight(this ISwipeView swipeView, SwipeDirection? swipeDirection, UIView contentView)
		{
			var items = GetSwipeItemsByDirection(swipeView, swipeDirection);

			if (items != null)
			{
				foreach (var item in items)
				{
					if (item is ISwipeItemView)
					{
						var itemsHeight = new List<double>();

						double itemHeight = 0;

						foreach (var swipeItem in items)
						{
							if (swipeItem is ISwipeItemView swipeItemView)
							{
								var swipeItemViewSizeRequest = swipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);

								if (swipeItemViewSizeRequest.Height > itemHeight)
									itemHeight = swipeItemViewSizeRequest.Height;
							}
						}

						return itemHeight;
					}
				}
			}

			return contentView.Frame.Height;
		}
	}
}
