#nullable disable
using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class GroupableItemsViewDelegator<TItemsView, TViewController> : SelectableItemsViewDelegator<TItemsView, TViewController>
		where TItemsView : GroupableItemsView
		where TViewController : GroupableItemsViewController<TItemsView>
	{
		public GroupableItemsViewDelegator(ItemsViewLayout itemsViewLayout, TViewController itemsViewController)
			: base(itemsViewLayout, itemsViewController)
		{
		}

		public override CGSize GetReferenceSizeForHeader(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return ViewController?.GetReferenceSizeForHeader(collectionView, layout, section) ?? CGSize.Empty;
		}

		public override CGSize GetReferenceSizeForFooter(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return ViewController?.GetReferenceSizeForFooter(collectionView, layout, section) ?? CGSize.Empty;
		}

		public override void ScrollAnimationEnded(UIScrollView scrollView)
		{
			ViewController?.HandleScrollAnimationEnded();
		}

		public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			if (ItemsViewLayout == null)
			{
				return default;
			}

			return ViewController?.GetInsetForSection(ItemsViewLayout, collectionView, section) ?? UIEdgeInsets.Zero;
		}
	}
}