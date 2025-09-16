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
				return proposedIndexPath;

			if (originalIndexPath.Section != proposedIndexPath.Section && !itemsView.CanMixGroups)
				return originalIndexPath;

			var itemsSource = ViewController?.ItemsSource;
			if (itemsSource == null)
				return proposedIndexPath;

			var totalSections = itemsSource.GroupCount;

			if (originalIndexPath.Equals(proposedIndexPath) && itemsView.CanMixGroups)
			{
				var emptyGroupTarget = FindFirstEmptyGroup(itemsSource, totalSections);
				if (emptyGroupTarget != null)
				{
					System.Diagnostics.Debug.WriteLine("Detected potential drop into empty section {0} (iOS reverted to original {1})", emptyGroupTarget.Section, originalIndexPath);
					return emptyGroupTarget;
				}
			}


			if (proposedIndexPath.Section >= totalSections)
				return originalIndexPath;

			var targetGroupItemCount = itemsSource.ItemCountInGroup(proposedIndexPath.Section);

			if (targetGroupItemCount == 0)
			{
				System.Diagnostics.Debug.WriteLine("Accepting drop into empty section {0}", proposedIndexPath.Section);
				return NSIndexPath.FromRowSection(0, proposedIndexPath.Section);
			}

			// Handle dropping past last item in current section
			if (proposedIndexPath.Row >= targetGroupItemCount)
			{
				// Check if we should redirect to next empty group
				if (proposedIndexPath.Section < totalSections - 1)
				{
					var nextSectionItemCount = itemsSource.ItemCountInGroup(proposedIndexPath.Section + 1);
					if (nextSectionItemCount == 0)
					{
						System.Diagnostics.Debug.WriteLine("Redirecting to next empty section {0}", proposedIndexPath.Section + 1);
						return NSIndexPath.FromRowSection(0, proposedIndexPath.Section + 1);
					}
				}

				// Clamp to last valid position in current section
				return NSIndexPath.FromRowSection(targetGroupItemCount, proposedIndexPath.Section);
			}

			// Proposed position is valid
			return proposedIndexPath;
		}

		private NSIndexPath FindFirstEmptyGroup(IItemsViewSource itemsSource, int totalSections)
		{
			for (int section = 0; section < totalSections; section++)
			{
				var sectionItemCount = itemsSource.ItemCountInGroup(section);
				if (sectionItemCount == 0)
					return NSIndexPath.FromRowSection(0, section);
			}
			return null;
		}
	}
}
