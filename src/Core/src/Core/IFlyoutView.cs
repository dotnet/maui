namespace Microsoft.Maui
{
	public interface IFlyoutView : IView
	{
		IView Flyout { get; }
		IView Detail { get; }
		bool IsPresented { get; set; }
		FlyoutBehavior FlyoutBehavior { get; }
		double FlyoutWidth { get; }
		bool IsGestureEnabled { get; }
	}
}
