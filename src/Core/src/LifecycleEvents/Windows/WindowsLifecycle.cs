namespace Microsoft.Maui.LifecycleEvents
{
	public static class WindowsLifecycle
	{
		public delegate void OnActivated(UI.Xaml.Window window, UI.Xaml.WindowActivatedEventArgs args);
		public delegate void OnClosed(UI.Xaml.Window window, UI.Xaml.WindowEventArgs args);
		public delegate void OnLaunched(UI.Xaml.Application application, UI.Xaml.LaunchActivatedEventArgs args);
		public delegate void OnVisibilityChanged(UI.Xaml.Window window, UI.Xaml.WindowVisibilityChangedEventArgs args);
	}
}