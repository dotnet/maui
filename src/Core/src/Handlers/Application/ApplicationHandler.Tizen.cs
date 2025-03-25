using Tizen.Applications;
using Tizen.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, CoreApplication>
	{
		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView.Exit();
		}

		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView?.RequestNewWindow(application, args as OpenWindowRequest);
		}

		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				(window.Handler?.PlatformView as Window)?.Dispose();
			}
		}

		public static partial void MapActivateWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				// TODO.
			}
		}
	}
}