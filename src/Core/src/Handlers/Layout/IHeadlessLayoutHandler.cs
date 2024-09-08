namespace Microsoft.Maui.Handlers;

internal interface IHeadlessLayoutHandler : ILayoutHandler
{
	void CreateSubviews(ref int targetIndex);
	void MoveSubviews(int targetIndex);
}