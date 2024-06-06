using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, UI.Xaml.Application>
	{
		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView.Exit();
		}

		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView?.CreatePlatformWindow(application, args as OpenWindowRequest);
		}

		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				var platformWindow = window.Handler?.PlatformView as Window;

				if (platformWindow is not null)
				{
					var handle = WinRT.Interop.WindowNative.GetWindowHandle(platformWindow);
					var id = UI.Win32Interop.GetWindowIdFromWindow(handle);
					var appWindow = UI.Windowing.AppWindow.GetFromWindowId(id);

					// Cannot close an already closed, or disposed Window.
					if (appWindow is not null && appWindow.IsVisible)
					{
						platformWindow.Close();
					}
				}
			}
		}
	}
}