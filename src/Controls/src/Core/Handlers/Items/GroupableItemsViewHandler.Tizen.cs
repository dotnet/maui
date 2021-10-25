namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> : SelectableItemsViewHandler<TItemsView> where TItemsView : GroupableItemsView
	{
		
		protected override Tizen.UIExtensions.NUI.CollectionView CreateNativeView()
		{
			throw new NotImplementedException();
		}

		public static void MapIsGrouped(GroupableItemsViewHandler<TItemsView> handler, GroupableItemsView itemsView)
		{
		}
	}
}
