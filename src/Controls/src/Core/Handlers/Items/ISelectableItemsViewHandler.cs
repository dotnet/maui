namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial interface ISelectableItemsViewHandler : IStructuredItemsViewHandler
	{
		new SelectableItemsView VirtualView { get; }
	}
}
