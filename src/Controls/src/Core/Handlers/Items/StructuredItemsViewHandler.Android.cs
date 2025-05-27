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
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateLayoutManager();

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
			=> (handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateAdapter();

		//TODO Make this public in .NET10
		internal static void MapItemsLayoutPropertyChanged(StructuredItemsViewHandler<TItemsView> handler, TItemsView view, object args)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateItemsLayoutProperties(args);
		}
	}
}
