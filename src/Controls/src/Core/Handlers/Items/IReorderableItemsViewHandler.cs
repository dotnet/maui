namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial interface IReorderableItemsViewHandler : IGroupableItemsViewHandler
	{
		new ReorderableItemsView VirtualView { get; }
	}
}
