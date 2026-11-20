#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class ItemsViewDelegator2<TItemsView, TViewController> : UICollectionViewDelegateFlowLayout, IScrollTrackingDelegator
		where TItemsView : ItemsView
		where TViewController : ItemsViewController2<TItemsView>
	{
		readonly WeakReference<TViewController> _viewController;
		WeakReference<IItemsViewSource> _thresholdNotificationSource;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		public UICollectionViewLayout ItemsViewLayout { get; }
		public TViewController ViewController => _viewController.TryGetTarget(out var vc) ? vc : null;

		protected float PreviousHorizontalOffset, PreviousVerticalOffset;

		public ItemsViewDelegator2(UICollectionViewLayout itemsViewLayout, TViewController ItemsViewController2)
		{
			ItemsViewLayout = itemsViewLayout;
			_viewController = new(ItemsViewController2);
		}

		void IScrollTrackingDelegator.ResetScrollTracking()
		{
			PreviousHorizontalOffset = 0;
			PreviousVerticalOffset = 0;
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			var (visibleItems, firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex) = GetVisibleItemsIndex();

			var contentInset = scrollView.ContentInset;
			var contentOffsetX = !visibleItems ? 0 : scrollView.ContentOffset.X + contentInset.Left;
			var contentOffsetY = !visibleItems ? 0 : scrollView.ContentOffset.Y + contentInset.Top;

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
			if (itemsView is null || source is null)
				return;

			itemsView.SendScrolled(itemsViewScrolledEventArgs);

			PreviousHorizontalOffset = (float)contentOffsetX;
			PreviousVerticalOffset = (float)contentOffsetY;

			if (!visibleItems || viewController.ItemsSource != source ||
				!IsThresholdReached(itemsView.RemainingItemsThreshold, source.ItemCount, lastVisibleItemIndex))
			{
				return;
			}

			if (_thresholdNotificationSource is not null)
			{
				_thresholdNotificationSource.SetTarget(source);
				return;
			}

			// A threshold handler can update the source, which is unsafe during UIKit's scroll/layout callback.
			_thresholdNotificationSource = new WeakReference<IItemsViewSource>(source);
			DispatchQueue.MainQueue.DispatchAsync(DeliverThresholdNotification);
		}

		static bool IsThresholdReached(int threshold, int itemCount, int lastVisibleItemIndex) =>
			threshold switch
			{
				-1 => false,
				0 => lastVisibleItemIndex == itemCount - 1,
				_ => itemCount - 1 - lastVisibleItemIndex <= threshold
			};

		void DeliverThresholdNotification()
		{
			try
			{
				var viewController = ViewController;
				var itemsView = viewController?.ItemsView;
				var source = viewController?.ItemsSource;
				if (itemsView is null || source is null ||
					!_thresholdNotificationSource.TryGetTarget(out var pendingSource) || source != pendingSource ||
					itemsView.Handler?.PlatformView != viewController.View ||
					viewController.CollectionView?.Delegate != this)
				{
					return;
				}

				var (visibleItems, _, _, lastVisibleItemIndex) = GetVisibleItemsIndex();
				if (visibleItems && IsThresholdReached(itemsView.RemainingItemsThreshold, source.ItemCount, lastVisibleItemIndex))
				{
					itemsView.SendRemainingItemsThresholdReached();
				}
			}
			finally
			{
				_thresholdNotificationSource = null;
			}
		}

#if MACCATALYST
		public override void DraggingStarted(UIScrollView scrollView)
		{
			if (scrollView is MauiCollectionView mauiCV)
			{
				mauiCV.ClearPendingScrollRestore();
			}
		}
#endif

		public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout,
			nint section)
		{
			if (ItemsViewLayout == null)
			{
				return default;
			}

			return default; // TODO: FIx  ItemsViewLayout.GetInsetForSection(collectionView, layout, section);
		}

		public override nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView,
			UICollectionViewLayout layout, nint section)
		{
			if (ItemsViewLayout == null)
			{
				return default;
			}

			return default; // TODO: Fix ItemsViewLayout.GetMinimumInteritemSpacingForSection(collectionView, layout, section);
		}

		public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView,
			UICollectionViewLayout layout, nint section)
		{
			if (ItemsViewLayout == null)
			{
				return default;
			}

			return default; // TODO: Fix ItemsViewLayout.GetMinimumLineSpacingForSection(collectionView, layout, section);
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

			// Sort visible item index paths by section and then by row for consistent order in both grouped and ungrouped sources
			var indexPathsForVisibleItems = collectionView.IndexPathsForVisibleItems.OrderBy(x => x.Section).ThenBy(x => x.Row).ToList();

			var visibleItems = indexPathsForVisibleItems.Count > 0;
			NSIndexPath firstVisibleItemIndex = null, centerItemIndex = null, lastVisibleItemIndex = null;

			if (visibleItems)
			{
				(firstVisibleItemIndex, lastVisibleItemIndex) = GetVisibleIndexPathsUsingLayoutAttributes(collectionView, indexPathsForVisibleItems);
				centerItemIndex = GetCenteredIndexPath(collectionView);
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

		static (NSIndexPath First, NSIndexPath Last) GetVisibleIndexPathsUsingLayoutAttributes(UICollectionView collectionView, IEnumerable<NSIndexPath> indexPathsForVisibleItems)
		{
			if (!indexPathsForVisibleItems.Any())
				return default;

			var layout = collectionView.CollectionViewLayout;
			if (layout is null)
				return (indexPathsForVisibleItems.First(), indexPathsForVisibleItems.Last());

			var visibleRect = new CGRect(collectionView.ContentOffset, collectionView.Bounds.Size);
			var layoutAttributes = layout.LayoutAttributesForElementsInRect(visibleRect);
			if (layoutAttributes is null || layoutAttributes.Length == 0)
				return (indexPathsForVisibleItems.First(), indexPathsForVisibleItems.Last());

			var flowLayout = layout as UICollectionViewFlowLayout;
			bool isVertical = flowLayout?.ScrollDirection != UICollectionViewScrollDirection.Horizontal;
			// Find the first visible cell (not headers/footers) based on scroll direction
			NSIndexPath firstVisibleIndexPath = null, lastVisibleIndexPath = null;
			nfloat minPosition = nfloat.MaxValue;

			for (int i = 0; i < layoutAttributes.Length; i++)
			{
				var attr = layoutAttributes[i];
				// Skip non-cell elements (headers, footers, decorations)
				if (attr.RepresentedElementCategory != UICollectionElementCategory.Cell)
					continue;

				// Skip items that don't intersect with visible rect
				if (!attr.Frame.IntersectsWith(visibleRect))
					continue;

				nfloat position = isVertical ? attr.Frame.Y : attr.Frame.X;
				if (position < minPosition)
				{
					minPosition = position;
					firstVisibleIndexPath = attr.IndexPath;
				}

				// The native visible-cell cache can lag behind the current viewport during Scrolled.
				var indexPath = attr.IndexPath;
				if (lastVisibleIndexPath is null
					|| indexPath.Section > lastVisibleIndexPath.Section
					|| (indexPath.Section == lastVisibleIndexPath.Section && indexPath.Item > lastVisibleIndexPath.Item))
				{
					lastVisibleIndexPath = indexPath;
				}
			}

			return (firstVisibleIndexPath ?? indexPathsForVisibleItems.First(),
				lastVisibleIndexPath ?? indexPathsForVisibleItems.Last());
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

		// public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
		// {
		// 	return ViewController?.GetSizeForItem(indexPath) ?? CGSize.Empty;
		// }
	}
}
