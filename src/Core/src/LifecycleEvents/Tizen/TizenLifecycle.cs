using Tizen.Applications;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class TizenLifecycle
	{
		// Events called by CoreUIApplication overrides
		public delegate void OnPause(CoreApplication application);
		public delegate void OnPreCreate(CoreApplication application);
		public delegate void OnResume(CoreApplication application);

		// Events called by CoreApplication overrides
		public delegate void OnAppControlReceived(CoreApplication application, AppControlReceivedEventArgs e);
		public delegate void OnCreate(CoreApplication application);
		public delegate void OnDeviceOrientationChanged(CoreApplication application, DeviceOrientationEventArgs e);
		public delegate void OnLocaleChanged(CoreApplication application, LocaleChangedEventArgs e);
		public delegate void OnLowBattery(CoreApplication application, LowBatteryEventArgs e);
		public delegate void OnLowMemory(CoreApplication application, LowMemoryEventArgs e);
		public delegate void OnRegionFormatChanged(CoreApplication application, RegionFormatChangedEventArgs e);
		public delegate void OnTerminate(CoreApplication application);

		// Internal events
		internal delegate void OnMauiContextCreated(IMauiContext mauiContext);
	}
}