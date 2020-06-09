using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using WRect = Windows.Foundation.Rect;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class CollectionViewExtensions
	{
		public static bool IsListViewItemVisible(FrameworkElement element, FrameworkElement container, ItemsLayoutOrientation itemsLayoutOrientation)
		{
			if (element == null || container == null)
				return false;

			if (element.Visibility != Visibility.Visible)
				return false;

			var elementBounds = element.TransformToVisual(container).TransformBounds(new WRect(0, 0, element.ActualWidth, element.ActualHeight));
			var containerBounds = new WRect(0, 0, container.ActualWidth, container.ActualHeight);

			switch (itemsLayoutOrientation)
			{

				case ItemsLayoutOrientation.Vertical:
					return elementBounds.Top < containerBounds.Bottom && elementBounds.Bottom > containerBounds.Top;

				default:
					return elementBounds.Left < containerBounds.Right && elementBounds.Right > containerBounds.Left;
			};
		}

		public static (int firstVisibleItemIndex, int lastVisibleItemIndex, int centerItemIndex) GetVisibleIndexes(this ListViewBase listViewBase, ItemsLayoutOrientation itemsLayoutOrientation , bool goingNext)
		{
			int firstVisibleItemIndex = -1;
			int lastVisibleItemIndex = -1;
			
			var itemsPanel = (listViewBase.ItemsPanelRoot as ItemsStackPanel);
			if (itemsPanel != null)
			{
				firstVisibleItemIndex = itemsPanel.FirstVisibleIndex;
				lastVisibleItemIndex = itemsPanel.LastVisibleIndex;
			}
			else
			{
				var scrollViewer = listViewBase.GetFirstDescendant<ScrollViewer>();
				var presenters = listViewBase.GetChildren<ListViewItemPresenter>();

				if (presenters != null || scrollViewer == null)
				{
					int count = 0;
					foreach (ListViewItemPresenter presenter in presenters)
					{
						if (IsListViewItemVisible(presenter, scrollViewer, itemsLayoutOrientation))
						{
							if (firstVisibleItemIndex == -1)
								firstVisibleItemIndex = count;

							lastVisibleItemIndex = count;
						}

						count++;
					}
				}
			}

			double center = (lastVisibleItemIndex + firstVisibleItemIndex) / 2.0;
			int centerItemIndex = goingNext ? (int)Math.Ceiling(center) : (int)Math.Floor(center);

			return (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex);
		}
	}
}
