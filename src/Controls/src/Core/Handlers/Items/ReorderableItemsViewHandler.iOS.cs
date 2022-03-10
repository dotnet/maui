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

		public static void MapCanReorderItems(IReorderableItemsViewHandler handler, ReorderableItemsView itemsView)
		{
			((handler as ReorderableItemsViewHandler<TItemsView>)?.Controller as ReorderableItemsViewController<TItemsView>)?.UpdateCanReorderItems();
		}
	}
}
