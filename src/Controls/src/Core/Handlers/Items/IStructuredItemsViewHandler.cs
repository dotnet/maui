namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial interface IStructuredItemsViewHandler : IItemsViewHandler
	{ 
		new StructuredItemsView VirtualView { get; }
	}
}
