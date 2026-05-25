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
