#nullable disable
using TCollectionView = Gtk.UIExtensions.NUI.CollectionView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> : SelectableItemsViewHandler<TItemsView> where TItemsView : GroupableItemsView
	{
		protected override TCollectionView CreatePlatformView()
		{
			return new MauiGroupableItemsView<TItemsView>();
		}

		public static void MapIsGrouped(GroupableItemsViewHandler<TItemsView> handler, GroupableItemsView itemsView)
		{
			(handler.PlatformView as MauiGroupableItemsView<TItemsView>).UpdateAdaptor();
		}
	}
}
