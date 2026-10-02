#nullable disable
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	[Obsolete("This type is obsolete on iOS and Mac Catalyst. Use Microsoft.Maui.Controls.Handlers.Items2.SelectableItemsViewDelegator2<TItemsView, TViewController> instead.")]
	public class SelectableItemsViewDelegator<TItemsView, TViewController> : ItemsViewDelegator<TItemsView, TViewController>
		where TItemsView : SelectableItemsView
		where TViewController : SelectableItemsViewController<TItemsView>
	{
		public SelectableItemsViewDelegator(ItemsViewLayout itemsViewLayout, TViewController itemsViewController)
			: base(itemsViewLayout, itemsViewController)
		{
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			ViewController?.ItemSelected(collectionView, indexPath);
		}

		public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			ViewController?.ItemDeselected(collectionView, indexPath);
		}
	}
}