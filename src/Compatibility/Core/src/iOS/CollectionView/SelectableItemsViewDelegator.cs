using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
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