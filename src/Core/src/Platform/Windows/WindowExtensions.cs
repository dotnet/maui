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

		public static void UpdateWidth(this UI.Xaml.Window nativeWindow, IWindow window)
		{
			if (Primitives.Dimension.IsExplicitSet(window.Width))
				nativeWindow.SetSize(window.Width, window.Frame.Height);
		}

		public static void UpdateHeight(this UI.Xaml.Window nativeWindow, IWindow window)
		{
			if (Primitives.Dimension.IsExplicitSet(window.Height))
				nativeWindow.SetSize(window.Frame.Width, window.Height);
		}

		static void SetSize(this UI.Xaml.Window nativeWindow, double width, double height)
		{
			var hwnd = nativeWindow.GetWindowHandle();

			var dpi = NativeMethods.GetDpiForWindow(hwnd);
			var scalingFactor = dpi / 96.0;

			var pixelWidth = (int)(width * scalingFactor);
			var pixelHeight = (int)(height * scalingFactor);

			System.Diagnostics.Debug.WriteLine($"BEFORE: {nativeWindow.Bounds} ({width}, {height})");

			NativeMethods.SetWindowPos(
				hwnd,
				NativeMethods.SpecialWindowHandles.HWND_TOP,
				0, 0,
				pixelWidth, pixelHeight,
				NativeMethods.SetWindowPosFlags.SWP_NOMOVE);

			System.Diagnostics.Debug.WriteLine($"AFTER: {nativeWindow.Bounds} ({width}, {height})");
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