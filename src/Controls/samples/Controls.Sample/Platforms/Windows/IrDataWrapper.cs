namespace Microsoft.Maui
{
	internal record IrDataWrapper (IMauiContext context, PositionInfo position, object data, PositionalViewSelector positionalViewSelector, IVirtualListView virtualListView);
}
