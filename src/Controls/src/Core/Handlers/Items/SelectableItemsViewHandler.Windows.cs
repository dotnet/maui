using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}
	}
}
