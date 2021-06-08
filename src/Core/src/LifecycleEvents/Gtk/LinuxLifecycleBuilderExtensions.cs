namespace Microsoft.Maui.LifecycleEvents
{
	public static class LinuxLifecycleBuilderExtensions
	{
		public static ILinuxLifecycleBuilder OnActivated(this ILinuxLifecycleBuilder lifecycle, LinuxLifecycle.OnShown del) => lifecycle.OnEvent(del);
		public static ILinuxLifecycleBuilder OnClosed(this ILinuxLifecycleBuilder lifecycle, LinuxLifecycle.OnHidden del) => lifecycle.OnEvent(del);
		public static ILinuxLifecycleBuilder OnLaunched(this ILinuxLifecycleBuilder lifecycle, LinuxLifecycle.OnStartup del) => lifecycle.OnEvent(del);
		public static ILinuxLifecycleBuilder OnVisibilityChanged(this ILinuxLifecycleBuilder lifecycle, LinuxLifecycle.OnVisibilityChanged del) => lifecycle.OnEvent(del);
	}
}