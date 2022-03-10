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

		public static void MapHeaderTemplate(IStructuredItemsViewHandler handler, StructuredItemsView itemsView)
		{
		}

		public static void MapFooterTemplate(IStructuredItemsViewHandler handler, StructuredItemsView itemsView)
		{
		}

		public static void MapItemsLayout(IStructuredItemsViewHandler handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateLayoutManager();

		public static void MapItemSizingStrategy(IStructuredItemsViewHandler handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateAdapter();
	}
}
