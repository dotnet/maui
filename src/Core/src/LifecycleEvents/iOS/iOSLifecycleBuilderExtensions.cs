namespace Microsoft.Maui.LifecycleEvents
{
	public static class iOSLifecycleBuilderExtensions
	{
		public static IiOSLifecycleBuilder ContinueUserActivity(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.ContinueUserActivity del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder DidEnterBackground(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.DidEnterBackground del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder FinishedLaunching(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.FinishedLaunching del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder OnActivated(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.OnActivated del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder OnResignActivation(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.OnResignActivation del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder OpenUrl(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.OpenUrl del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder PerformActionForShortcutItem(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.PerformActionForShortcutItem del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder WillEnterForeground(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.WillEnterForeground del) => lifecycle.OnEvent(del);
		public static IiOSLifecycleBuilder WillTerminate(this IiOSLifecycleBuilder lifecycle, iOSLifecycle.WillTerminate del) => lifecycle.OnEvent(del);
	}
}