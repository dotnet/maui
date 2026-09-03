namespace Microsoft.Maui.LifecycleEvents
{
	public static class WindowsLifecycle
	{
		public delegate void OnActivated(UI.Xaml.Window window, UI.Xaml.WindowActivatedEventArgs args);
		public delegate void OnClosed(UI.Xaml.Window window, UI.Xaml.WindowEventArgs args);
		public delegate void OnLaunched(UI.Xaml.Application application, UI.Xaml.LaunchActivatedEventArgs args);
		public delegate void OnLaunching(UI.Xaml.Application application, UI.Xaml.LaunchActivatedEventArgs args);
		public delegate void OnVisibilityChanged(UI.Xaml.Window window, UI.Xaml.WindowVisibilityChangedEventArgs args);
		public delegate void OnPlatformMessage(UI.Xaml.Window window, WindowsPlatformMessageEventArgs args);
		public delegate void OnWindowCreated(UI.Xaml.Window window);
		public delegate void OnResumed(UI.Xaml.Window window);
		public delegate void OnPlatformWindowSubclassed(UI.Xaml.Window window, WindowsPlatformWindowSubclassedEventArgs args);

		// Internal events
		internal delegate void OnMauiContextCreated(IMauiContext mauiContext);
	}
}
