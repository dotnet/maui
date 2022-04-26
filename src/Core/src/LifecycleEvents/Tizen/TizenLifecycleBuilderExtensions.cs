namespace Microsoft.Maui.LifecycleEvents
{
	public static class TizenLifecycleBuilderExtensions
	{
		public static ITizenLifecycleBuilder OnPause(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnPause del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnPreCreate(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnPreCreate del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnResume(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnResume del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnAppControlReceived(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnAppControlReceived del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnCreate(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnCreate del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnDeviceOrientationChanged(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnDeviceOrientationChanged del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnLocaleChanged(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnLocaleChanged del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnLowBattery(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnLowBattery del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnLowMemory(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnLowMemory del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnRegionFormatChanged(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnRegionFormatChanged del) => lifecycle.OnEvent(del);
		public static ITizenLifecycleBuilder OnTerminate(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnTerminate del) => lifecycle.OnEvent(del);

		internal static ITizenLifecycleBuilder OnMauiContextCreated(this ITizenLifecycleBuilder lifecycle, TizenLifecycle.OnMauiContextCreated del) => lifecycle.OnEvent(del);
	}
}