using System;
using Microsoft.Maui.Essentials;
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
	}
}