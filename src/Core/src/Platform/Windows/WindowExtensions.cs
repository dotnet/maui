using System;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Media;
using WinRT.Interop;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		public static void UpdateTitle(this UI.Xaml.Window platformWindow, IWindow window)
		{
			platformWindow.Title = window.Title;

			var rootManager = window.Handler?.MauiContext?.GetNavigationRootManager();
			if (rootManager != null)
			{
				rootManager.SetWindowTitle(window.Title);
			}
		}

		public static IWindow? GetWindow(this UI.Xaml.Window platformWindow)
		{
			foreach (var window in MauiWinUIApplication.Current.Application.Windows)
			{
				if (window?.Handler?.PlatformView is UI.Xaml.Window win && win == platformWindow)
					return window;
			}

			return null;
		}

		public static IntPtr GetWindowHandle(this UI.Xaml.Window platformWindow)
		{
			var hwnd = WindowNative.GetWindowHandle(platformWindow);

			if (hwnd == IntPtr.Zero)
				throw new NullReferenceException("The Window Handle is null.");

			return hwnd;
		}

		public static float GetDisplayDensity(this UI.Xaml.Window platformWindow)
		{
			var hwnd = platformWindow.GetWindowHandle();

			if (hwnd == IntPtr.Zero)
				return 1.0f;

			return PlatformMethods.GetDpiForWindow(hwnd) / DeviceDisplay.BaseLogicalDpi;
		}

		public static UI.Windowing.AppWindow? GetAppWindow(this UI.Xaml.Window platformWindow)
		{
			var hwnd = platformWindow.GetWindowHandle();

			if (hwnd == IntPtr.Zero)
				return null;

			var windowId = UI.Win32Interop.GetWindowIdFromWindow(hwnd);
			return UI.Windowing.AppWindow.GetFromWindowId(windowId);
		}

		public static async Task<RenderedView?> RenderAsImage(this IWindow window, RenderType type)
		{
			if (window.Handler?.PlatformView is not UI.Xaml.Window win)
				return null;

			if (win.Content is not UI.Xaml.FrameworkElement element)
				return null;

			var image = type switch
			{
				RenderType.JPEG => await element.RenderAsJPEGAsync(),
				RenderType.PNG => await element.RenderAsPNGAsync(),
				RenderType.BMP => await element.RenderAsBMPAsync(),
				_ => throw new NotImplementedException(),
			};

			return new RenderedView(image, type);
		}
	}
}