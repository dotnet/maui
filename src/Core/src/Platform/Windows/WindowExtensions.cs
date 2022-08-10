using System;
using Microsoft.Maui.Devices;
using Windows.Graphics;
using Windows.Graphics.Display;
using WinRT.Interop;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		public static void UpdateTitle(this UI.Xaml.Window platformWindow, IWindow window)
		{
			platformWindow.Title = window.Title;

			if (platformWindow is MauiWinUIWindow mauiWindow)
				mauiWindow.UpdateTitleOnCustomTitleBar();
		}

		public static void UpdateX(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdatePosition(window.X, window.Y);

		public static void UpdateY(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdatePosition(window.X, window.Y);

		public static void UpdatePosition(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdatePosition(window.X, window.Y);

		internal static void UpdatePosition(this UI.Xaml.Window platformWindow, double x, double y)
		{
			var window = platformWindow.GetAppWindow();
			if (window is null)
				return;

			var density = platformWindow.GetDisplayDensity();

			var currPos = window.Position;
			x = Primitives.Dimension.IsExplicitSet(x)
				? Math.Round(x * density)
				: currPos.X;
			y = Primitives.Dimension.IsExplicitSet(y)
				? Math.Round(y * density)
				: currPos.Y;

			var pos = new PointInt32(
				(int)x,
				(int)y);

			if (pos != currPos)
				window.Move(pos);
		}

		public static void UpdateWidth(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateSize(window.Width, window.Height);

		public static void UpdateHeight(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateSize(window.Width, window.Height);

		public static void UpdateSize(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateSize(window.Width, window.Height);

		internal static void UpdateSize(this UI.Xaml.Window platformWindow, double width, double height)
		{
			var window = platformWindow.GetAppWindow();
			if (window is null)
				return;

			var density = platformWindow.GetDisplayDensity();

			var currSize = window.Size;
			width = Primitives.Dimension.IsExplicitSet(width)
				? Math.Round(width * density)
				: currSize.Width;
			height = Primitives.Dimension.IsExplicitSet(height)
				? Math.Round(height * density)
				: currSize.Height;

			var size = new SizeInt32(
				(int)width,
				(int)height);

			if (size != currSize)
				window.Resize(size);
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

		internal static DisplayOrientation GetOrientation(this IWindow? window)
		{
			if (window == null)
				return DeviceDisplay.Current.MainDisplayInfo.Orientation;

			var appWindow = window.Handler?.MauiContext?.GetPlatformWindow()?.GetAppWindow();

			if (appWindow == null)
				return DisplayOrientation.Unknown;

			DisplayOrientations orientationEnum;
			int theScreenWidth = appWindow.Size.Width;
			int theScreenHeight = appWindow.Size.Height;
			if (theScreenWidth > theScreenHeight)
				orientationEnum = DisplayOrientations.Landscape;
			else
				orientationEnum = DisplayOrientations.Portrait;

			return orientationEnum == DisplayOrientations.Landscape
				? DisplayOrientation.Landscape
				: DisplayOrientation.Portrait;
		}
	}
}