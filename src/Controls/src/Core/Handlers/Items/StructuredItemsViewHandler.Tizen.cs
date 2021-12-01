using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.NativeView.UpdateAdaptor(itemsView);
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.NativeView.UpdateAdaptor(itemsView);
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.NativeView.UpdateItemsLayout(itemsView);
		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.NativeView.UpdateItemsLayout(itemsView);
		}
	}
}
