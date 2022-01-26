using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> : SelectableItemsViewHandler<TItemsView> where TItemsView : GroupableItemsView
	{
		protected override TCollectionView CreateNativeView()
		{
			return new MauiGroupableItemsView<TItemsView>();
		}

		public static void MapIsGrouped(GroupableItemsViewHandler<TItemsView> handler, GroupableItemsView itemsView)
		{
			(handler.NativeView as MauiGroupableItemsView<TItemsView>).UpdateAdaptor();
		}
	}
}
