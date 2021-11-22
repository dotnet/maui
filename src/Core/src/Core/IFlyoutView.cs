namespace Microsoft.Maui
{
	public interface IFlyoutView : IView
	{
		IView Flyout { get; }
		IView Detail { get; }
	}
}
