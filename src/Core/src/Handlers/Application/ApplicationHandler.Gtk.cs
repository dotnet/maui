using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, Gtk.Application>
	{
		public static void MapTerminate(ApplicationHandler handler, IApplication application, object? args) { }
		public static void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args) { }
		public static void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args) { }
	}
}