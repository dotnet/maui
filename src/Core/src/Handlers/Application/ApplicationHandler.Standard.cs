using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, object>
	{
		protected override object CreateNativeElement() => throw new NotImplementedException();

		public static void MapTerminate(ApplicationHandler handler, IApplication application, object? args) { }
		public static void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args) { }
		public static void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args) { }
	}
}