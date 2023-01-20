using System;
using Microsoft.Maui.ApplicationModel;
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
			platformWindow.UpdatePosition(window);

		public static void UpdateY(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdatePosition(window);

		public static void UpdatePosition(this UI.Xaml.Window platformWindow, IWindow window)
		{
			var appWindow = platformWindow.GetAppWindow();
			if (appWindow is null)
				return;

			var density = platformWindow.GetDisplayDensity();
			var x = window.X;
			var y = window.Y;

			var currPos = appWindow.Position;
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
				appWindow.Move(pos);
		}

		public static void UpdateWidth(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateSize(window);

		public static void UpdateHeight(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateSize(window);

		public static void UpdateSize(this UI.Xaml.Window platformWindow, IWindow window)
		{
			var appWindow = platformWindow.GetAppWindow();
			if (appWindow is null)
				return;

			var density = platformWindow.GetDisplayDensity();
			var width = window.Width;
			var height = window.Height;

			var currSize = appWindow.Size;
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
				appWindow.Resize(size);
		}

		public static void UpdateMinimumWidth(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateMinimumSize(window);

		public static void UpdateMinimumHeight(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateMinimumSize(window);

		public static void UpdateMinimumSize(this UI.Xaml.Window platformWindow, IWindow window)
		{
			if (platformWindow is not IPlatformSizeRestrictedWindow restrictedWindow)
				return;

			var density = platformWindow.GetDisplayDensity();
			var minWidth = window.MinimumWidth;
			var minHeight = window.MinimumHeight;

			var actualMinWidth = double.IsFinite(minWidth)
				? (int)Math.Clamp(minWidth * density, 0, int.MaxValue)
				: 0;

			var actualMinHeight = double.IsFinite(minHeight)
				? (int)Math.Clamp(minHeight * density, 0, int.MaxValue)
				: 0;

			var minSize = new SizeInt32(
				actualMinWidth,
				actualMinHeight);

			restrictedWindow.MinimumSize = minSize;

			var appWindow = platformWindow.GetAppWindow();
			if (appWindow is null)
				return;

			var currentSize = appWindow.Size;
			var temp = currentSize;
			if (currentSize.Width < actualMinWidth)
				temp.Width = actualMinWidth;
			if (currentSize.Height < actualMinHeight)
				temp.Height = actualMinHeight;
			if (currentSize != temp)
				appWindow.Resize(temp);
		}

		public static void UpdateMaximumWidth(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateMaximumSize(window);

		public static void UpdateMaximumHeight(this UI.Xaml.Window platformWindow, IWindow window) =>
			platformWindow.UpdateMaximumSize(window);

		public static void UpdateMaximumSize(this UI.Xaml.Window platformWindow, IWindow window)
		{
			if (platformWindow is not IPlatformSizeRestrictedWindow restrictedWindow)
				return;

			var density = platformWindow.GetDisplayDensity();
			var maxWidth = window.MaximumWidth;
			var maxHeight = window.MaximumHeight;

			var actualMaxWidth = double.IsFinite(maxWidth)
				? (int)Math.Clamp(maxWidth * density, 0, int.MaxValue)
				: int.MaxValue;

			var actualMaxHeight = double.IsFinite(maxHeight)
				? (int)Math.Clamp(maxHeight * density, 0, int.MaxValue)
				: int.MaxValue;

			var MaxSize = new SizeInt32(
				actualMaxWidth,
				actualMaxHeight);

			restrictedWindow.MaximumSize = MaxSize;

			var appWindow = platformWindow.GetAppWindow();
			if (appWindow is null)
				return;

			var currentSize = appWindow.Size;
			var temp = currentSize;
			if (currentSize.Width > actualMaxWidth)
				temp.Width = actualMaxWidth;
			if (currentSize.Height > actualMaxHeight)
				temp.Height = actualMaxHeight;
			if (currentSize != temp)
				appWindow.Resize(temp);
		}

		public static IWindow? GetWindow(this UI.Xaml.Window platformWindow)
		{
			foreach (var window in WindowExtensions.GetWindows())
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