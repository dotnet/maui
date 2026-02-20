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
			NSIndexPath targetIndexPath;

			var itemsView = ViewController?.ItemsView;
			if (itemsView?.IsGrouped == true)
			{
				if (originalIndexPath.Section == proposedIndexPath.Section || itemsView.CanMixGroups)
				{
					targetIndexPath = proposedIndexPath;
				}
				else
				{
					targetIndexPath = originalIndexPath;
				}
			}
			else
			{
				targetIndexPath = proposedIndexPath;
			}

			return targetIndexPath;
		}
	}
}
