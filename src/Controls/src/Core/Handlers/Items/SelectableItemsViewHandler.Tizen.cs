#nullable disable
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		protected override TCollectionView CreatePlatformView()
		{
			return new MauiSelectableItemsView<TItemsView>();
		}

		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.PlatformView as MauiSelectableItemsView<TItemsView>)?.UpdateSelection();
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.PlatformView as MauiSelectableItemsView<TItemsView>)?.UpdateSelection();
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
			(handler.PlatformView as MauiSelectableItemsView<TItemsView>)?.UpdateSelection();
		}
	}
}
