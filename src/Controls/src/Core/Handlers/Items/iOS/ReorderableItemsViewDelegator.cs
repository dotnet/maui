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

			var itemsSource = ViewController?.ItemsSource;
			if (itemsSource == null)
			{
				return proposedIndexPath;
			}

			return ReorderableItemsViewExtensions.GetTargetIndexPathForGroupedMove(
				originalIndexPath,
				proposedIndexPath,
				itemsView,
				itemsSource,
				ViewController.HasInteractivelyMoved);
		}
	}
}
