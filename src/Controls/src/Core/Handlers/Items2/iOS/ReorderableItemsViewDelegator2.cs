#nullable disable
using System;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class ReorderableItemsViewDelegator2<TItemsView, TViewController> : GroupableItemsViewDelegator2<TItemsView, TViewController>
		where TItemsView : ReorderableItemsView
		where TViewController : ReorderableItemsViewController2<TItemsView>
	{
		public ReorderableItemsViewDelegator2(UICollectionViewLayout itemsViewLayout, TViewController ItemsViewController2)
			: base(itemsViewLayout, ItemsViewController2)
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

			if (originalIndexPath.Equals(proposedIndexPath) && itemsView.CanMixGroups)
			{
				var emptyGroupTarget = FindFirstEmptyGroup(itemsSource, totalSections);
				if (emptyGroupTarget != null)
				{
					return emptyGroupTarget;
				}
			}


			if (proposedIndexPath.Section >= totalSections)
				return originalIndexPath;

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

		NSIndexPath FindFirstEmptyGroup(IItemsViewSource itemsSource, int totalSections)
		{
			for (int section = 0; section < totalSections; section++)
			{
				var sectionItemCount = itemsSource.ItemCountInGroup(section);
				if (sectionItemCount == 0)
				{
					return NSIndexPath.FromRowSection(0, section);
				}
			}
			return null;
		}
	}
}
