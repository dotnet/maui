#nullable disable

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// Windows-specific handler for ReorderableItemsView in Items2 (CollectionView2).
	/// </summary>
	public partial class ReorderableItemsViewHandler2<TItemsView> : ItemsViewHandler2<ReorderableItemsView>
	{
		public static void MapCanReorderItems(ReorderableItemsViewHandler2<TItemsView> handler, ReorderableItemsView itemsView)
		{
			if (handler.PlatformView is MauiItemsView mauiItemsView)
			{
				mauiItemsView.UpdateCanReorderItems(itemsView.CanReorderItems);
			}
		}
	}
}
