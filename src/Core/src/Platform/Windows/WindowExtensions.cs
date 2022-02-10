using System;
using WinRT.Interop;

namespace Microsoft.Maui.Platform
{
	public static class WindowExtensions
	{
		public static void UpdateTitle(this UI.Xaml.Window nativeWindow, IWindow window)
		{
			nativeWindow.Title = window.Title;

			var rootManager = window.Handler?.MauiContext?.GetNavigationRootManager();
			if (rootManager != null)
			{
				rootManager.SetWindowTitle(window.Title);
			}
		}

		public static IWindow? GetWindow(this UI.Xaml.Window nativeWindow)
		{
			foreach (var window in MauiWinUIApplication.Current.Application.Windows)
			{
				if (window?.Handler?.NativeView is UI.Xaml.Window win && win == nativeWindow)
					return window;
			}

			return null;
		}

		public static IntPtr GetWindowHandle(this UI.Xaml.Window nativeWindow)
		{
			var hwnd = WindowNative.GetWindowHandle(nativeWindow);

			if (hwnd == IntPtr.Zero)
				throw new NullReferenceException("The Window Handle is null.");

			return hwnd;
		}
	}
}