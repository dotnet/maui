#nullable disable
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ReorderableItemsViewDelegator<TItemsView, TViewController> : GroupableItemsViewDelegator<TItemsView, TViewController>
		where TItemsView : ReorderableItemsView
		where TViewController : ReorderableItemsViewController<TItemsView>
	{
		public ReorderableItemsViewDelegator(ItemsViewLayout itemsViewLayout, TViewController itemsViewController)
			: base(itemsViewLayout, itemsViewController)
		{
		}

		public override NSIndexPath GetTargetIndexPathForMove(UICollectionView collectionView, NSIndexPath originalIndexPath, NSIndexPath proposedIndexPath)
		{
			var itemsView = ViewController?.ItemsView;

			if (itemsView?.IsGrouped != true)
			{
				return proposedIndexPath;
			}

			if (originalIndexPath.Section != proposedIndexPath.Section && !itemsView.CanMixGroups)
			{
				return originalIndexPath;
			}

			var itemsSource = ViewController?.ItemsSource;
			if (itemsSource == null)
			{
				return proposedIndexPath;
			}

			var totalSections = itemsSource.GroupCount;

			// UICollectionView falls back to proposedIndexPath == originalIndexPath when the
			// user drags over an area with no cells (e.g. an empty group's header region).
			// In that case, redirect to the nearest empty group so the drop can succeed.
			if (originalIndexPath.Equals(proposedIndexPath) && itemsView.CanMixGroups)
			{
				var emptyGroupTarget = FindNearestEmptyGroup(itemsSource, totalSections, originalIndexPath.Section);
				if (emptyGroupTarget != null)
				{
					return emptyGroupTarget;
				}
			}

			if (proposedIndexPath.Section >= totalSections)
			{
				return originalIndexPath;
			}

			var targetGroupItemCount = itemsSource.ItemCountInGroup(proposedIndexPath.Section);

			if (targetGroupItemCount == 0)
			{
				return NSIndexPath.FromRowSection(0, proposedIndexPath.Section);
			}

			if (proposedIndexPath.Row >= targetGroupItemCount)
			{
				if (proposedIndexPath.Section < totalSections - 1)
				{
					var nextSectionItemCount = itemsSource.ItemCountInGroup(proposedIndexPath.Section + 1);
					if (nextSectionItemCount == 0)
					{
						return NSIndexPath.FromRowSection(0, proposedIndexPath.Section + 1);
					}
				}

				return NSIndexPath.FromRowSection(targetGroupItemCount, proposedIndexPath.Section);
			}

			return proposedIndexPath;
		}

		static NSIndexPath FindNearestEmptyGroup(IItemsViewSource itemsSource, int totalSections, nint currentSection)
		{
			// Search outward from the current section to find the closest empty group.
			for (int offset = 1; offset < totalSections; offset++)
			{
				var before = (int)currentSection - offset;
				if (before >= 0 && itemsSource.ItemCountInGroup(before) == 0)
				{
					return NSIndexPath.FromRowSection(0, before);
				}

				var after = (int)currentSection + offset;
				if (after < totalSections && itemsSource.ItemCountInGroup(after) == 0)
				{
					return NSIndexPath.FromRowSection(0, after);
				}
			}
			return null;
		}
	}
}
