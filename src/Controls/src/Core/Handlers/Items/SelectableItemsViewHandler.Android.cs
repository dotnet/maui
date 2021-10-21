using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView>
		where TItemsView : SelectableItemsView
	{
		protected override SelectableItemsViewAdapter<TItemsView, IItemsViewSource> CreateAdapter() => new(VirtualView);

		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			handler.NativeView.UpdateSelection(itemsView);
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			handler.NativeView.UpdateSelection(itemsView);
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			handler.NativeView.UpdateSelection(itemsView);
		}
	}
}
