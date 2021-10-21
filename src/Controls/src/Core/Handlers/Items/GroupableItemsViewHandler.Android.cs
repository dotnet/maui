using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using AndroidX.RecyclerView.Widget;


namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> : SelectableItemsViewHandler<TItemsView>
		where TItemsView : GroupableItemsView
	{
		protected override Android.Views.View CreateNativeView()
		{
			throw new NotImplementedException();
		}

		public static void MapIsGrouped(GroupableItemsViewHandler<TItemsView> handler, GroupableItemsView itemsView)
		{
		}



		//protected override RecyclerView CreateNativeView()
		//{
		//	var recycler = new MauiRecyclerView<TItemsView,
		//		GroupableItemsViewAdapter<TItemsView, IGroupableItemsViewSource>, IItemsViewSource>(Context, GetItemsLayout, 
		//		() => new GroupableItemsViewAdapter<TItemsView, IGroupableItemsViewSource>(VirtualView), VirtualView);
		//	return recycler;
		//}
	}
}
