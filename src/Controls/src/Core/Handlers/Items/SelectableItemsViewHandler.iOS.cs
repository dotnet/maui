namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		protected override ItemsViewController<TItemsView> CreateController(TItemsView itemsView, ItemsViewLayout layout)
			=> new SelectableItemsViewController<TItemsView>(itemsView, layout);

		public static void MapItemsSource(ISelectableItemsViewHandler handler, SelectableItemsView itemsView)
		{
			ItemsViewHandler<TItemsView>.MapItemsSource(handler, itemsView);
			MapSelectedItem(handler, itemsView);
		}

		public static void MapSelectedItem(ISelectableItemsViewHandler handler, SelectableItemsView itemsView)
		{
			((handler as SelectableItemsViewHandler<TItemsView>)?.Controller as SelectableItemsViewController<TItemsView>)?.UpdatePlatformSelection();
		}

		public static void MapSelectedItems(ISelectableItemsViewHandler handler, SelectableItemsView itemsView)
		{
			((handler as SelectableItemsViewHandler<TItemsView>)?.Controller as SelectableItemsViewController<TItemsView>)?.UpdatePlatformSelection();
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			((handler as SelectableItemsViewHandler<TItemsView>)?.Controller as SelectableItemsViewController<TItemsView>)?.UpdateSelectionMode();
		}
	}
}
