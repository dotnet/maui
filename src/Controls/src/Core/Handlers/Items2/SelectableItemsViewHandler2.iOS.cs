#nullable disable
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class SelectableItemsViewHandler2<TItemsView> : StructuredItemsViewHandler2<TItemsView> where TItemsView : SelectableItemsView
	{
		protected override ItemsViewController2<TItemsView> CreateController(TItemsView itemsView, UICollectionViewLayout layout)
			=> new SelectableItemsViewController2<TItemsView>(itemsView, layout);

		public static void MapItemsSource(SelectableItemsViewHandler2<TItemsView> handler, SelectableItemsView itemsView)
		{
			ItemsViewHandler2<TItemsView>.MapItemsSource(handler, itemsView);
			MapSelectedItem(handler, itemsView);
		}

		public static void MapSelectedItem(SelectableItemsViewHandler2<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController2<TItemsView>)?.UpdatePlatformSelection();
		}

		public static void MapSelectedItems(SelectableItemsViewHandler2<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController2<TItemsView>)?.UpdatePlatformSelection();
		}

		public static void MapSelectionMode(SelectableItemsViewHandler2<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController2<TItemsView>)?.UpdateSelectionMode();
		}
	}
}
