#nullable disable
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class GroupableItemsViewHandler2<TItemsView> : SelectableItemsViewHandler2<TItemsView> where TItemsView : GroupableItemsView
	{
		protected override ItemsViewController2<TItemsView> CreateController(TItemsView itemsView, UICollectionViewLayout layout)
			 => new GroupableItemsViewController2<TItemsView>(itemsView, layout);

		protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			if (WillNeedScrollAdjustment(args))
			{
				if (args.IsAnimated)
				{
					(Controller as GroupableItemsViewController2<TItemsView>).SetScrollAnimationEndedCallback(() => base.ScrollToRequested(sender, args));
				}
				else
				{
					base.ScrollToRequested(sender, args);
				}
			}

			base.ScrollToRequested(sender, args);
		}

		public static void MapIsGrouped(GroupableItemsViewHandler2<TItemsView> handler, GroupableItemsView itemsView)
		{
			handler.Controller?.UpdateItemsSource();
		}

		bool WillNeedScrollAdjustment(ScrollToRequestEventArgs args)
		{
			return ItemsView.ItemSizingStrategy == ItemSizingStrategy.MeasureAllItems
				&& ItemsView.IsGrouped
				&& (args.ScrollToPosition == ScrollToPosition.End || args.ScrollToPosition == ScrollToPosition.MakeVisible);
		}
	}
}
