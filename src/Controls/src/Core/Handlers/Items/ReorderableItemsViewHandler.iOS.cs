#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class ReorderableItemsViewHandler<TItemsView> : GroupableItemsViewHandler<TItemsView> where TItemsView : ReorderableItemsView
	{
		protected override ItemsViewController<TItemsView> CreateController(TItemsView itemsView, ItemsViewLayout layout)
			 => new ReorderableItemsViewController<TItemsView>(itemsView, layout);

		public static void MapCanReorderItems(ReorderableItemsViewHandler<TItemsView> handler, ReorderableItemsView itemsView)
		{
			(handler.Controller as ReorderableItemsViewController<TItemsView>)?.UpdateCanReorderItems();
		}
	}
}
