#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.RecyclerView.Widget;


namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> : SelectableItemsViewHandler<TItemsView>
		where TItemsView : GroupableItemsView
	{
		new protected virtual GroupableItemsViewAdapter<TItemsView, IGroupableItemsViewSource> CreateAdapter() => new(VirtualView);

		protected override RecyclerView CreatePlatformView() =>
			new MauiRecyclerView<TItemsView, GroupableItemsViewAdapter<TItemsView, IGroupableItemsViewSource>, IGroupableItemsViewSource>(Context, GetItemsLayout, CreateAdapter);

		public static void MapIsGrouped(GroupableItemsViewHandler<TItemsView> handler, GroupableItemsView itemsView)
		{
			(handler.PlatformView as IMauiRecyclerView<TItemsView>)?.UpdateItemsSource();
		}
	}
}
