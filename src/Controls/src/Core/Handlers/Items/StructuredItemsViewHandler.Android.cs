#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		protected override IItemsLayout GetItemsLayout() => VirtualView.ItemsLayout;

		protected override StructuredItemsViewAdapter<TItemsView, IItemsViewSource> CreateAdapter() => new(VirtualView);

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			if (handler.PlatformView is IMauiRecyclerView<TItemsView> recyclerView)
			{
				recyclerView.UpdateAdapter();
				recyclerView.UpdateScrollingMode();
				recyclerView.UpdateLayoutManager();
			}
		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateAdapter();
	}
}
