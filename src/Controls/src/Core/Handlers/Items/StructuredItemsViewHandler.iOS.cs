#nullable disable
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override ItemsViewController<TItemsView> CreateController(TItemsView itemsView, UICollectionViewLayout layout)
				=> new StructuredItemsViewController<TItemsView>(itemsView, layout);

		protected override UICollectionViewLayout SelectLayout()
		{
			var itemSizingStrategy = ItemsView.ItemSizingStrategy;
			var itemsLayout = ItemsView.ItemsLayout;

			if (itemsLayout is GridItemsLayout gridItemsLayout)
			{
				return LayoutFactory.CreateGrid(gridItemsLayout);
			}

			if (itemsLayout is LinearItemsLayout listItemsLayout)
			{
				return LayoutFactory.CreateList(listItemsLayout);
			}
			
			// Fall back to vertical list
			return LayoutFactory.CreateList(new LinearItemsLayout(ItemsLayoutOrientation.Vertical));
		}

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.Controller as StructuredItemsViewController<TItemsView>)?.UpdateHeaderView();
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			(handler.Controller as StructuredItemsViewController<TItemsView>)?.UpdateFooterView();
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}
	}
}
