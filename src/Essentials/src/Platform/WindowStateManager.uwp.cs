#nullable enable
using System;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IWindowStateManager
	{
		event EventHandler ActiveWindowChanged;

		event EventHandler ActiveWindowDisplayChanged;

		// TODO: NET7 make this public
		// event EventHandler ActiveWindowThemeChanged;

		Window? GetActiveWindow();

		void OnActivated(Window window, WindowActivatedEventArgs args);

		void OnWindowMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}

	public static class WindowStateManager
	{
		static IWindowStateManager? defaultImplementation;

		public static IWindowStateManager Default =>
			defaultImplementation ??= new WindowStateManagerImplementation();

		internal static void SetDefault(IWindowStateManager? implementation) =>
			defaultImplementation = implementation;
	}

	static class WindowStateManagerExtensions
	{
		public static Window? GetActiveWindow(this IWindowStateManager manager, bool throwOnNull)
		{
			var window = manager.GetActiveWindow();
			if (throwOnNull && window == null)
				throw new NullReferenceException("The active Window can not be detected. Ensure that you have called Init in your Application class.");

			return window;
		}

		public static IntPtr GetActiveWindowHandle(this IWindowStateManager manager, bool throwOnNull)
		{
			var window = manager.GetActiveWindow();
			if (throwOnNull && window == null)
				throw new NullReferenceException("The active Window can not be detected. Ensure that you have called Init in your Application class.");

			if (window == null)
				return IntPtr.Zero;

			var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);

			return handle;
		}

		public static AppWindow? GetActiveAppWindow(this IWindowStateManager manager, bool throwOnNull)
		{
			var window = manager.GetActiveWindow();
			if (throwOnNull && window == null)
				throw new NullReferenceException("The active Window can not be detected. Ensure that you have called Init in your Application class.");

			if (window == null)
				return null;

			var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
			var windowId = UI.Win32Interop.GetWindowIdFromWindow(handle);
			var appWindow = AppWindow.GetFromWindowId(windowId);

			return appWindow;
		}
	}

	class WindowStateManagerImplementation : IWindowStateManager
	{
		const uint WM_DISPLAYCHANGE = 0x7E;
		const uint WM_DPICHANGED = 0x02E0;
		const uint WM_SETTINGCHANGE = 0x001A;
		const uint WM_THEMECHANGE = 0x031A;

		Window? _activeWindow;
		IntPtr _activeWindowHandle;

		public event EventHandler? ActiveWindowChanged;

		public event EventHandler? ActiveWindowDisplayChanged;

		public event EventHandler? ActiveWindowThemeChanged;

		public Window? GetActiveWindow() =>
			_activeWindow;

		void SetActiveWindow(Window window)
		{
			if (_activeWindow == window)
				return;

			_activeWindow = window;
			_activeWindowHandle = window is null
				? IntPtr.Zero
				: WinRT.Interop.WindowNative.GetWindowHandle(window);

			ActiveWindowChanged?.Invoke(window, EventArgs.Empty);
		}

		public void OnActivated(Window window, WindowActivatedEventArgs args)
		{
			if (args.WindowActivationState != WindowActivationState.Deactivated)
				SetActiveWindow(window);
		}

		// Currently there isn't a way to detect Orientation Changes unless you subclass the WinUI.Window and watch the messages
		// Maui.Core forwards these messages to here so that WinUI can react accordingly.
		// This is the "subtlest" way to currently wire this together. 
		// Hopefully there will be a more public API for this down the road so we can just use that directly from Essentials
		public void OnWindowMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			// only track events if they come from the active window
			if (_activeWindow is null || hWnd != _activeWindowHandle)
				return;

			if (msg == WM_SETTINGCHANGE || msg == WM_THEMECHANGE)
				ActiveWindowThemeChanged?.Invoke(_activeWindow, EventArgs.Empty);
			else if (msg == WM_DISPLAYCHANGE || msg == WM_DPICHANGED)
				ActiveWindowDisplayChanged?.Invoke(_activeWindow, EventArgs.Empty);
		}
	}
}
