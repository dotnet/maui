namespace Microsoft.Maui.LifecycleEvents
{

	public static class GtkLifecycleBuilderExtensions
	{

		public static IGtkLifecycleBuilder OnActivated(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnApplicationActivated del) => lifecycle.OnEvent(del);

		public static IGtkLifecycleBuilder OnLaunching(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnLaunching del) => lifecycle.OnEvent(del);

		public static IGtkLifecycleBuilder OnLaunched(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnLaunched del) => lifecycle.OnEvent(del);

		public static IGtkLifecycleBuilder OnCreated(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnCreated del) => lifecycle.OnEvent(del);

		public static IGtkLifecycleBuilder OnShown(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnShown del) => lifecycle.OnEvent(del);

		public static IGtkLifecycleBuilder OnStateChanged(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnStateChanged del) => lifecycle.OnEvent(del);

		public static IGtkLifecycleBuilder OnClosed(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnHidden del) => lifecycle.OnEvent(del);

		public static IGtkLifecycleBuilder OnDelete(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnDelete del) => lifecycle.OnEvent(del);

		internal static IGtkLifecycleBuilder OnMauiContextCreated(this IGtkLifecycleBuilder lifecycle, GtkLifecycle.OnMauiContextCreated del) => lifecycle.OnEvent(del);

	}

}