#nullable disable
using System;
using Foundation;
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
