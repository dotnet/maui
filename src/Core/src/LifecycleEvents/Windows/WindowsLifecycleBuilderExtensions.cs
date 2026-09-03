namespace Microsoft.Maui.LifecycleEvents
{
	public static class WindowsLifecycleBuilderExtensions
	{
		public static IWindowsLifecycleBuilder OnActivated(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnActivated del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnClosed(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnClosed del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnLaunching(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnLaunching del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnLaunched(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnLaunched del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnVisibilityChanged(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnVisibilityChanged del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnWindowCreated(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnWindowCreated del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnResumed(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnResumed del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnPlatformMessage(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnPlatformMessage del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnPlatformWindowSubclassed(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnPlatformWindowSubclassed del) => lifecycle.OnEvent(del);

		internal static IWindowsLifecycleBuilder OnMauiContextCreated(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnMauiContextCreated del) => lifecycle.OnEvent(del);
	}
}