namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface IGroupableItemsViewHandler : ISelectableItemsViewHandler
	{
		new GroupableItemsView VirtualView { get; }
	}
}
