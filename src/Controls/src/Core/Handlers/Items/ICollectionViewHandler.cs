namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial interface ICollectionViewHandler : IReorderableItemsViewHandler
	{
		new CollectionView VirtualView { get; }
	}
}
