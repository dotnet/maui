using Tizen.Applications;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, CoreApplication>
	{
		public static void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView.Exit();
		}

		public static void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView?.RequestNewWindow(application, args as OpenWindowRequest);
		}

		public static void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				//TODO : Need to implementation
			}
		}
	}
}