#nullable disable
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override ItemsViewController<TItemsView> CreateController(TItemsView itemsView, ItemsViewLayout layout)
				=> new StructuredItemsViewController<TItemsView>(itemsView, layout);

		protected override ItemsViewLayout SelectLayout()
		{
			var itemSizingStrategy = ItemsView.ItemSizingStrategy;
			var itemsLayout = ItemsView.ItemsLayout;

			if (itemsLayout is GridItemsLayout gridItemsLayout)
			{
				return new GridViewLayout(gridItemsLayout, itemSizingStrategy);
			}

			if (itemsLayout is LinearItemsLayout listItemsLayout)
			{
				return new ListViewLayout(listItemsLayout, itemSizingStrategy);
			}

			// Fall back to vertical list
			return new ListViewLayout(new LinearItemsLayout(ItemsLayoutOrientation.Vertical), itemSizingStrategy);
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

		//TODO Make this public in .NET10
		internal static void MapItemsLayoutPropertyChanged(StructuredItemsViewHandler<TItemsView> handler, TItemsView view, object args)
		{
			handler.UpdateLayout();
		}
	}
}
