namespace Microsoft.Maui.LifecycleEvents
{
	public static class AndroidLifecycleBuilderExtensions
	{
		public static IAndroidLifecycleBuilder OnActivityResult(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnActivityResult del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnBackPressed(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnBackPressed del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnConfigurationChanged(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnConfigurationChanged del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnCreate(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnCreate del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnDestroy(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnDestroy del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnNewIntent(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnNewIntent del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnPause(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnPause del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnPostCreate(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnPostCreate del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnPostResume(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnPostResume del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnPressingBack(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnPressingBack del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnRequestPermissionsResult(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnRequestPermissionsResult del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnRestart(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnRestart del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnRestoreInstanceState(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnRestoreInstanceState del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnResume(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnResume del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnSaveInstanceState(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnSaveInstanceState del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnStart(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnStart del) => lifecycle.OnEvent(del);
		public static IAndroidLifecycleBuilder OnStop(this IAndroidLifecycleBuilder lifecycle, AndroidLifecycle.OnStop del) => lifecycle.OnEvent(del);
	}
}