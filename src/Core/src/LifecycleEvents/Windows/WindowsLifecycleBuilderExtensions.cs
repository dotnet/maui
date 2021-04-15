namespace Microsoft.Maui.LifecycleEvents
{
	public static class WindowsLifecycleBuilderExtensions
	{
		public static IWindowsLifecycleBuilder OnActivated(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnActivated del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnClosed(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnClosed del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnLaunched(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnLaunched del) => lifecycle.OnEvent(del);
		public static IWindowsLifecycleBuilder OnVisibilityChanged(this IWindowsLifecycleBuilder lifecycle, WindowsLifecycle.OnVisibilityChanged del) => lifecycle.OnEvent(del);
	}
}