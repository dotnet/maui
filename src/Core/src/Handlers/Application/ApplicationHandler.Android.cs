using System;
using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, Application>
	{
		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			var currentActivity = ApplicationModel.Platform.CurrentActivity;

			if (currentActivity != null)
			{
				currentActivity.FinishAndRemoveTask();

				Environment.Exit(0);
			}
		}

		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView?.RequestNewWindow(application, args as OpenWindowRequest);
		}

		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				if (window.Handler?.PlatformView is Activity activity)
					activity.Finish();
			}
		}

		public static partial void MapActivateWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				// TODO.
			}
		}

		internal static partial void MapAppTheme(ApplicationHandler handler, IApplication application)
		{
			application?.UpdateNightMode();
		}
	}
}