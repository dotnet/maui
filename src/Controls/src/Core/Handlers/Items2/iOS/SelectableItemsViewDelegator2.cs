#nullable disable
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class SelectableItemsViewDelegator2<TItemsView, TViewController> : ItemsViewDelegator2<TItemsView, TViewController>
		where TItemsView : SelectableItemsView
		where TViewController : SelectableItemsViewController2<TItemsView>
	{
		public SelectableItemsViewDelegator2(UICollectionViewLayout itemsViewLayout, TViewController itemsViewController)
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