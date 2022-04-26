using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			if (itemsView.SelectionMode != SelectionMode.None && itemsView.SelectedItem != null)
			{
				var index = handler.PlatformView.Adaptor.GetItemIndex(itemsView.SelectedItem);
				handler.PlatformView.SelectedItemIndex = index;
			}
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			handler.PlatformView.SelectionMode = itemsView.SelectionMode.ToPlatform();
		}
	}
}
