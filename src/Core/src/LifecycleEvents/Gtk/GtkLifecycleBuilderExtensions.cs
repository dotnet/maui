namespace Microsoft.Maui.LifecycleEvents
{
	public static class GtkLifecycleBuilderExtensions
	{
		public static IGtkLifecycleBuilder OnActivated(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnShown del) => lifecycle.OnEvent(del);
		public static IGtkLifecycleBuilder OnClosed(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnHidden del) => lifecycle.OnEvent(del);
		public static IGtkLifecycleBuilder OnLaunched(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnStartup del) => lifecycle.OnEvent(del);
		public static IGtkLifecycleBuilder OnVisibilityChanged(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnVisibilityChanged del) => lifecycle.OnEvent(del);
		public static IGtkLifecycleBuilder OnShown(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnShown del) => lifecycle.OnEvent(del);
	}
}