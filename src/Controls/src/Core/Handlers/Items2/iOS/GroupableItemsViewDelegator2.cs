#nullable disable
using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class GroupableItemsViewDelegator2<TItemsView, TViewController> : SelectableItemsViewDelegator2<TItemsView, TViewController>
		where TItemsView : GroupableItemsView
		where TViewController : GroupableItemsViewController2<TItemsView>
	{
		public GroupableItemsViewDelegator2(UICollectionViewLayout itemsViewLayout, TViewController ItemsViewController2)
			: base(itemsViewLayout, ItemsViewController2)
		{
		}

		// public override CGSize GetReferenceSizeForHeader(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		// {
		// 	return ViewController?.GetReferenceSizeForHeader(collectionView, layout, section) ?? CGSize.Empty;
		// }
		//
		// public override CGSize GetReferenceSizeForFooter(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		// {
		// 	return ViewController?.GetReferenceSizeForFooter(collectionView, layout, section) ?? CGSize.Empty;
		// }

		public override void ScrollAnimationEnded(UIScrollView scrollView)
		{
			ViewController?.HandleScrollAnimationEnded();
		}

		// public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		// {
		// 	if (ItemsViewLayout == null)
		// 	{
		// 		return default;
		// 	}
		//
		// 	return ViewController?.GetInsetForSection(ItemsViewLayout, collectionView, section) ?? UIEdgeInsets.Zero;
		// }
	}
}