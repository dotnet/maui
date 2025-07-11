#nullable disable
using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ItemsViewDelegator<TItemsView, TViewController> : UICollectionViewDelegateFlowLayout
		where TItemsView : ItemsView
		where TViewController : ItemsViewController<TItemsView>
	{
		readonly WeakReference<TViewController> _viewController;

		public ItemsViewLayout ItemsViewLayout { get; }
		public TViewController ViewController => _viewController.TryGetTarget(out var vc) ? vc : null;

		protected float PreviousHorizontalOffset, PreviousVerticalOffset;

		public ItemsViewDelegator(ItemsViewLayout itemsViewLayout, TViewController itemsViewController)
		{
			ItemsViewLayout = itemsViewLayout;
			_viewController = new(itemsViewController);
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			var (visibleItems, firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex) = GetVisibleItemsIndex();

			if (!visibleItems && !HasHeaderOrFooter())
			{
				return;
			}

			var contentInset = scrollView.ContentInset;
			var contentOffsetX = scrollView.ContentOffset.X + contentInset.Left;
			var contentOffsetY = scrollView.ContentOffset.Y + contentInset.Top;

			var itemsViewScrolledEventArgs = new ItemsViewScrolledEventArgs
			{
				HorizontalDelta = contentOffsetX - PreviousHorizontalOffset,
				VerticalDelta = contentOffsetY - PreviousVerticalOffset,
				HorizontalOffset = contentOffsetX,
				VerticalOffset = contentOffsetY,
				FirstVisibleItemIndex = firstVisibleItemIndex,
				CenterItemIndex = centerItemIndex,
				LastVisibleItemIndex = lastVisibleItemIndex
			};

			var viewController = ViewController;
			if (viewController is null)
				return;

			var itemsView = viewController.ItemsView;
			var source = viewController.ItemsSource;
			itemsView.SendScrolled(itemsViewScrolledEventArgs);

			PreviousHorizontalOffset = (float)contentOffsetX;
			PreviousVerticalOffset = (float)contentOffsetY;

			switch (itemsView.RemainingItemsThreshold)
			{
				case -1:
					return;
				case 0:
					if (lastVisibleItemIndex == source.ItemCount - 1)
						itemsView.SendRemainingItemsThresholdReached();
					break;
				default:
					if (source.ItemCount - 1 - lastVisibleItemIndex <= itemsView.RemainingItemsThreshold)
						itemsView.SendRemainingItemsThresholdReached();
					break;
			}
		}

		public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout,
			nint section)
		{
			if (ItemsViewLayout == null)
			{
				return default;
			}

			return ItemsViewLayout.GetInsetForSection(collectionView, layout, section);
		}

		public override nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView,
			UICollectionViewLayout layout, nint section)
		{
			if (ItemsViewLayout == null)
			{
				return default;
			}

			return ItemsViewLayout.GetMinimumInteritemSpacingForSection(collectionView, layout, section);
		}

		public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView,
			UICollectionViewLayout layout, nint section)
		{
			if (ItemsViewLayout == null)
			{
				return default;
			}

			return ItemsViewLayout.GetMinimumLineSpacingForSection(collectionView, layout, section);
		}

		public override void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
		{
			ViewController?.CellDisplayingEndedFromDelegate(cell, indexPath);
		}

		protected virtual (bool VisibleItems, NSIndexPath First, NSIndexPath Center, NSIndexPath Last) GetVisibleItemsIndexPath()
		{
			var collectionView = ViewController?.CollectionView;
			if (collectionView is null)
				return default;

			var indexPathsForVisibleItems = collectionView.IndexPathsForVisibleItems.OrderBy(x => x.Row).ToList();

			var visibleItems = indexPathsForVisibleItems.Count > 0;
			NSIndexPath firstVisibleItemIndex = null, centerItemIndex = null, lastVisibleItemIndex = null;

			if (visibleItems)
			{
				firstVisibleItemIndex = indexPathsForVisibleItems.First();
				centerItemIndex = GetCenteredIndexPath(collectionView);
				lastVisibleItemIndex = indexPathsForVisibleItems.Last();
			}

			return (visibleItems, firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}

		protected virtual (bool VisibleItems, int First, int Center, int Last) GetVisibleItemsIndex()
		{
			var (VisibleItems, First, Center, Last) = GetVisibleItemsIndexPath();
			int firstVisibleItemIndex = -1, centerItemIndex = -1, lastVisibleItemIndex = -1;
			if (VisibleItems)
			{
				IItemsViewSource source = ViewController.ItemsSource;

				firstVisibleItemIndex = GetItemIndex(First, source);
				centerItemIndex = GetItemIndex(Center, source);
				lastVisibleItemIndex = GetItemIndex(Last, source);
			}
			return (VisibleItems, firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}

		bool HasHeaderOrFooter()
		{
			var viewController = ViewController;

			if (viewController?.ItemsView is null)
			{
				return false;
			}

			// Check for structured headers/footers
			if (viewController.ItemsView is StructuredItemsView structuredItemsView)
			{
				if (structuredItemsView.Header is not null || structuredItemsView.HeaderTemplate is not null ||
					structuredItemsView.Footer is not null || structuredItemsView.FooterTemplate is not null)
				{
					return true;
				}
			}

			// Check for group headers/footers
			if (viewController.ItemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
			{
				if (groupableItemsView.GroupHeaderTemplate is not null || groupableItemsView.GroupFooterTemplate is not null)
				{
					return true;
				}
			}

			return false;
		}

		static int GetItemIndex(NSIndexPath indexPath, IItemsViewSource itemSource)
		{
			int index = (int)indexPath.Item;

			if (indexPath.Section > 0)
			{
				for (int i = 0; i < indexPath.Section; i++)
				{
					index += itemSource.ItemCountInGroup(i);
				}
			}

			return index;
		}

		static NSIndexPath GetCenteredIndexPath(UICollectionView collectionView)
		{
			NSIndexPath centerItemIndex = null;

			var indexPathsForVisibleItems = collectionView.IndexPathsForVisibleItems.OrderBy(x => x.Row).ToList();

			if (indexPathsForVisibleItems.Count == 0)
				return centerItemIndex;

			var firstVisibleItemIndex = indexPathsForVisibleItems.First();

			var centerPoint = new CGPoint(collectionView.Center.X + collectionView.ContentOffset.X, collectionView.Center.Y + collectionView.ContentOffset.Y);
			var centerIndexPath = collectionView.IndexPathForItemAtPoint(centerPoint);
			centerItemIndex = centerIndexPath ?? firstVisibleItemIndex;
			return centerItemIndex;
		}

		public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
		{
			return ViewController?.GetSizeForItem(indexPath) ?? CGSize.Empty;
		}
	}
}