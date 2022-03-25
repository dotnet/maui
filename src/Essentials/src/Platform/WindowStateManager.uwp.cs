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
		const uint DISPLAY_CHANGED = 126;
		const uint DPI_CHANGED = 736;

		Window? _activeWindow;

		public event EventHandler? ActiveWindowChanged;

		public event EventHandler? ActiveWindowDisplayChanged;

		public Window? GetActiveWindow() =>
			_activeWindow;

		void SetActiveWindow(Window window)
		{
			if (_activeWindow == window)
				return;

			_activeWindow = window;
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
			if (ActiveWindowDisplayChanged == null)
				return;

			// We only care about orientation or dpi changes
			if (DISPLAY_CHANGED != msg && DPI_CHANGED != msg)
				return;

			if (_activeWindow != null && hWnd == WinRT.Interop.WindowNative.GetWindowHandle(_activeWindow))
				ActiveWindowDisplayChanged?.Invoke(_activeWindow, EventArgs.Empty);
		}
	}
}
