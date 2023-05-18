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
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		protected override ItemsViewController<TItemsView> CreateController(TItemsView itemsView, ItemsViewLayout layout)
			=> new SelectableItemsViewController<TItemsView>(itemsView, layout);

		public static void MapItemsSource(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			ItemsViewHandler<TItemsView>.MapItemsSource(handler, itemsView);
			MapSelectedItem(handler, itemsView);
		}

		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController<TItemsView>)?.UpdatePlatformSelection();
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController<TItemsView>)?.UpdatePlatformSelection();
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController<TItemsView>)?.UpdateSelectionMode();
		}
	}
}
