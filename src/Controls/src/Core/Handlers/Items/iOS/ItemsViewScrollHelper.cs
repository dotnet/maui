using Foundation;
using Microsoft.Maui.Controls.Handlers.Items2;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class ItemsViewScrollHelper
	{
		internal static void ForceScrollToLastItem(UICollectionView collectionView, UICollectionViewScrollPosition position)
		{
			var sections = (int)collectionView.NumberOfSections();
			for (int section = sections - 1; section >= 0; section--)
			{
				var itemCount = collectionView.NumberOfItemsInSection(section);
				if (itemCount > 0)
				{
					var lastIndexPath = NSIndexPath.FromItemSection(itemCount - 1, section);
					collectionView.ScrollToItem(lastIndexPath, position, true);
					return;
				}
			}
		}

		internal static void ScrollToFirstItem(UICollectionView collectionView, ItemsLayoutOrientation orientation)
		{
			if (collectionView == null || collectionView.DataSource is CarouselViewController or CarouselViewController2)
			{
				return;
			}

			int sections = (int)collectionView.NumberOfSections();
			for (int section = 0; section < sections; section++)
			{
				if (collectionView.NumberOfItemsInSection(section) == 0)
				{
					continue;
				}

				var indexPath = NSIndexPath.FromItemSection(0, section);
				var position = orientation == ItemsLayoutOrientation.Vertical
					? UICollectionViewScrollPosition.Top
					: collectionView.EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft
						? UICollectionViewScrollPosition.Right
						: UICollectionViewScrollPosition.Left;

				var attributes = collectionView.CollectionViewLayout.LayoutAttributesForItem(indexPath);
				if (attributes != null && collectionView.Bounds.Contains(attributes.Frame))
				{
					return;
				}

				collectionView.ScrollToItem(indexPath, position, false);
				return;
			}
		}
	}
}
