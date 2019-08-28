using System.ComponentModel;

namespace Xamarin.Forms.Platform.iOS
{
	public class GroupableItemsViewRenderer : SelectableItemsViewRenderer
	{
		GroupableItemsView GroupableItemsView => (GroupableItemsView)Element;
		GroupableItemsViewController GroupableItemsViewController => (GroupableItemsViewController)ItemsViewController;

		protected override ItemsViewController CreateController(ItemsView itemsView, ItemsViewLayout layout)
		{
			return new GroupableItemsViewController(itemsView as GroupableItemsView, layout);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(GroupableItemsView.IsGroupedProperty))
			{
				GroupableItemsViewController?.UpdateItemsSource();
			}
		}

		// This is awkward.

		// Under certain circumstances, scrolling to an item in a group does not scroll all the way to the item; the 
		// UICollectionView will stop short. This only happens when:
		// 1. The items are grouped
		// 2. The target scroll position is "End" or "MakeVisible"
		// 3. The ItemSizingStrategy is "MeasureAllItems" (i.e., we are using `estimatedItemSize` for the underlying
		//		UICollectionView, rather than setting `itemSize` or using `sizeForItemAtIndexPath`.)
		// 4. The item has never appeared on screen before

		// This may be an iOS bug.

		// To work around this, we check below for the right conditions and, it they are met, we simply call 
		// ScrollToItem twice. The first time gets us close to the correct offset, the second time gets us all the way
		// there. If the scroll is _not_ animated, we can simply call it twice in a row; if it's animated, then we 
		// have to wait for the first scroll to finish before calling the second one (`SetScrollAnimationEndedCallback`).

		protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			if (WillNeedScrollAdjustment(args))
			{
				if (args.IsAnimated)
				{
					GroupableItemsViewController.SetScrollAnimationEndedCallback(() => base.ScrollToRequested(sender, args));
				}
				else
				{
					base.ScrollToRequested(sender, args);
				}
			}

			base.ScrollToRequested(sender, args);
		}

		bool WillNeedScrollAdjustment(ScrollToRequestEventArgs args)
		{
			return GroupableItemsView.ItemSizingStrategy == ItemSizingStrategy.MeasureAllItems
				&& GroupableItemsView.IsGrouped
				&& (args.ScrollToPosition == ScrollToPosition.End || args.ScrollToPosition == ScrollToPosition.MakeVisible);
		}
	}
}