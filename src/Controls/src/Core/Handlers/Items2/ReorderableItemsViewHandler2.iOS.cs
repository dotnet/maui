#nullable disable
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class ReorderableItemsViewHandler2<TItemsView> : GroupableItemsViewHandler2<TItemsView> where TItemsView : ReorderableItemsView
	{
		protected override ItemsViewController2<TItemsView> CreateController(TItemsView itemsView, UICollectionViewLayout layout)
			 => new ReorderableItemsViewController2<TItemsView>(itemsView, layout);

		public static void MapCanReorderItems(ReorderableItemsViewHandler2<TItemsView> handler, ReorderableItemsView itemsView)
		{
			(handler.Controller as ReorderableItemsViewController2<TItemsView>)?.UpdateCanReorderItems();
		}
	}
}
