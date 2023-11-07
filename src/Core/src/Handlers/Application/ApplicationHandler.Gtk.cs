using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, Gtk.Application>
	{
		[MissingMapper]
		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args) { }

		[MissingMapper]
		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args) { }

		[MissingMapper]
		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args) { }
	}
}