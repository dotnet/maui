namespace Microsoft.Maui.LifecycleEvents
{
	public static class AndroidLifecycleBuilderExtensions
	{
		public static IAndroidLifecycleBuilder OnApplicationCreating(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnApplicationCreating del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnApplicationCreate(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnApplicationCreate del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnApplicationLowMemory(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnApplicationLowMemory del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnApplicationTrimMemory(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnApplicationTrimMemory del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnApplicationConfigurationChanged(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnApplicationConfigurationChanged del) => lifecycle.OnEvent(del);

		public static IAndroidLifecycleBuilder OnActivityResult(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnActivityResult del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnBackPressed(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnBackPressed del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnConfigurationChanged(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnConfigurationChanged del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnCreate(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnCreate del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnDestroy(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnDestroy del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnKeyDown(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnKeyDown del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnKeyLongPress(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnKeyLongPress del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnKeyMultiple(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnKeyMultiple del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnKeyShortcut(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnKeyShortcut del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnKeyUp(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnKeyUp del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnNewIntent(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnNewIntent del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnPause(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnPause del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnPostCreate(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnPostCreate del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnPostResume(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnPostResume del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnRequestPermissionsResult(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnRequestPermissionsResult del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnRestart(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnRestart del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnRestoreInstanceState(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnRestoreInstanceState del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnResume(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnResume del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnSaveInstanceState(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnSaveInstanceState del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnStart(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnStart del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnStop(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnStop del) => lifecycle.OnEvent(del);

		internal static IAndroidLifecycleBuilder OnMauiContextCreated(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnMauiContextCreated del) => lifecycle.OnEvent(del);
	}
}