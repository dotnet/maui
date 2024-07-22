#nullable disable
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class LayoutGroupingInfo
	{
		public bool IsGrouped { get; set; }
		public bool HasHeader { get; set; }
		public bool HasFooter { get; set; }
	}

	public partial class StructuredItemsViewHandler2<TItemsView> : ItemsViewHandler2<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override ItemsViewController2<TItemsView> CreateController(TItemsView itemsView, UICollectionViewLayout layout)
				=> new StructuredItemsViewController2<TItemsView>(itemsView, layout);

		protected override UICollectionViewLayout SelectLayout()
		{
			var groupInfo = new LayoutGroupingInfo();
			
			if (ItemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
			{
				groupInfo.IsGrouped = groupableItemsView.IsGrouped;
				groupInfo.HasHeader = groupableItemsView.GroupHeaderTemplate is not null;
				groupInfo.HasFooter = groupableItemsView.GroupFooterTemplate is not null;
			}

			var itemSizingStrategy = ItemsView.ItemSizingStrategy;
			var itemsLayout = ItemsView.ItemsLayout;

			if (itemsLayout is GridItemsLayout gridItemsLayout)
			{
				return LayoutFactory.CreateGrid(gridItemsLayout, groupInfo);
			}

			if (itemsLayout is LinearItemsLayout listItemsLayout)
			{
				return LayoutFactory.CreateList(listItemsLayout, groupInfo);
			}
			
			// Fall back to vertical list
			return LayoutFactory.CreateList(new LinearItemsLayout(ItemsLayoutOrientation.Vertical), groupInfo);
		}

		public static void MapHeaderTemplate(StructuredItemsViewHandler2<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.Controller as StructuredItemsViewController2<TItemsView>)?.UpdateHeaderView();
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler2<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.Controller as StructuredItemsViewController2<TItemsView>)?.UpdateFooterView();
		}

		public static void MapItemsLayout(StructuredItemsViewHandler2<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler2<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}
	}
}
