using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

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

			var elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
			var containerBounds = new Rect(0, 0, container.ActualWidth, container.ActualHeight);

			switch (itemsLayoutOrientation)
			{

				case ItemsLayoutOrientation.Vertical:
					return elementBounds.Top < containerBounds.Bottom && elementBounds.Bottom > containerBounds.Top;

				default:
					return elementBounds.Left < containerBounds.Right && elementBounds.Right > containerBounds.Left;
			};
		}

		public static (int firstVisibleItemIndex, int lastVisibleItemIndex, int centerItemIndex) GetVisibleIndexes(ListViewBase listViewBase, ItemsLayoutOrientation itemsLayoutOrientation)
		{
			int firstVisibleItemIndex = -1;
			int lastVisibleItemIndex = -1;
			int centerItemIndex = -1;

			var scrollViewer = listViewBase.GetFirstDescendant<ScrollViewer>();
			var presenters = listViewBase.GetChildren<ListViewItemPresenter>();

			if (presenters != null || scrollViewer == null)
			{
				int count = 0;
				foreach (ListViewItemPresenter presenter in presenters)
				{
					if (CollectionViewExtensions.IsListViewItemVisible(presenter, scrollViewer, itemsLayoutOrientation))
					{
						if (firstVisibleItemIndex == -1)
							firstVisibleItemIndex = count;

						lastVisibleItemIndex = count;
					}

					count++;
				}

				centerItemIndex = (lastVisibleItemIndex + firstVisibleItemIndex) / 2;
			}

			return (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex);
		}
	}
}
