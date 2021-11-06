using Android.App;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, Application>
	{
		public static void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.Logger?.LogWarning("Android does not support programmatically terminating the app.");
		}

		public static void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.NativeView?.RequestNewWindow(application, args as OpenWindowRequest);
		}

		public static void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				var activity = application.Handler?.MauiContext?.GetActivity();

				if (activity != null)
				{
					activity.Finish();
				}
			}
		}

		public static void MapOnWindowClosed(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
				application.OnWindowClosed(window);
		}
	}
}