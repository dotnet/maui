namespace Microsoft.Maui.Controls.Handlers.Items;

// Implemented by delegators to allow explicit scroll-tracking reset when ItemsSource changes.
internal interface IScrollTrackingDelegator
{
	void ResetScrollTracking();
}
