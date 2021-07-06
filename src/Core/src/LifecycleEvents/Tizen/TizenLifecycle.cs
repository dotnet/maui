using Tizen.Applications;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class TizenLifecycle
	{
		// Events called by CoreUIApplication overrides
		public delegate void OnPause(CoreUIApplication application);
		public delegate void OnPreCreate(CoreUIApplication application);
		public delegate void OnResume(CoreUIApplication application);

		// Events called by CoreApplication overrides
		public delegate void OnAppControlReceived(CoreUIApplication application, AppControlReceivedEventArgs e);
		public delegate void OnCreate(CoreUIApplication application);
		public delegate void OnDeviceOrientationChanged(CoreUIApplication application, DeviceOrientationEventArgs e);
		public delegate void OnLocaleChanged(CoreUIApplication application, LocaleChangedEventArgs e);
		public delegate void OnLowBattery(CoreUIApplication application, LowBatteryEventArgs e);
		public delegate void OnLowMemory(CoreUIApplication application, LowMemoryEventArgs e);
		public delegate void OnRegionFormatChanged(CoreUIApplication application, RegionFormatChangedEventArgs e);
		public delegate void OnTerminate(CoreUIApplication application);

		// Internal events
		internal delegate void OnMauiContextCreated(IMauiContext mauiContext);
	}
}