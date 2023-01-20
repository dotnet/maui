#nullable enable
using System;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IWindowStateManager
	{
		event EventHandler ActiveWindowChanged;

		Window? GetActiveWindow();

		void OnActivated(Window window, WindowActivatedEventArgs args);
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
		Window? _activeWindow;

		public event EventHandler? ActiveWindowChanged;

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
	}
}
