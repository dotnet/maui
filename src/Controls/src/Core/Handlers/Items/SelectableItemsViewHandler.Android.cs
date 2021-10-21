using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using AndroidX.RecyclerView.Widget;


namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView>
		where TItemsView : SelectableItemsView
	{
		protected override Android.Views.View CreateNativeView()
		{
			return new SelectableItemsViewAdapter<TItemsView, IItemsViewSource>(VirtualView);
		}

		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			handler.UpdateNativeSelection();
		}

		void UpdateNativeSelection()
		{
			var mode = VirtualView.SelectionMode;

			//ItemsViewAdapter.ClearNativeSelection();

			//switch (mode)
			//{
			//	case SelectionMode.None:
			//		return;

			//	case SelectionMode.Single:
			//		var selectedItem = ItemsView.SelectedItem;
			//		ItemsViewAdapter.MarkNativeSelection(selectedItem);
			//		return;

			//	case SelectionMode.Multiple:
			//		var selectedItems = ItemsView.SelectedItems;

			//		foreach (var item in selectedItems)
			//		{
			//			ItemsViewAdapter.MarkNativeSelection(item);
			//		}
			//		return;
			//}
		}
	}
}
