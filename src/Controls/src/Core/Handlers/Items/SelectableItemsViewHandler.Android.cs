#nullable disable
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView>
		where TItemsView : SelectableItemsView
	{
		protected override SelectableItemsViewAdapter<TItemsView, IItemsViewSource> CreateAdapter() => new(VirtualView);

		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
			=> handler.PlatformView.UpdateSelection(itemsView);

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
			=> handler.PlatformView.UpdateSelection(itemsView);

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			var adapter = handler.PlatformView.GetAdapter() as ReorderableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>;
			adapter?.UpdateSelectionMode();
			
			handler.PlatformView.UpdateSelection(itemsView);
		}
	}
}
