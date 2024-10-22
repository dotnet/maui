#nullable enable
using System;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Manager object that manages window states on Windows.
	/// </summary>
	public interface IWindowStateManager
	{
		/// <summary>
		/// Occurs when the application's active window changed.
		/// </summary>
		event EventHandler ActiveWindowChanged;

		/// <summary>
		/// Gets the application's currently active window.
		/// </summary>
		/// <returns>The application's currently active <see cref="Window"/> object.</returns>
		Window? GetActiveWindow();

		/// <summary>
		/// Occurs when a new window is created, but not yet displayed
		/// </summary>
		/// <param name="window">The <see cref="Window"/> object</param>
		void OnPlatformWindowInitialized(Window window);

		/// <summary>
		/// Sets the new active window that can be retrieved with <see cref="GetActiveWindow"/>.
		/// </summary>
		/// <param name="window">The <see cref="Window"/> object that is activated.</param>
		/// <param name="args">The associated event arguments for this window activation event.</param>
		void OnActivated(Window window, WindowActivatedEventArgs args);
	}

	/// <summary>
	/// Manager object that manages window states on Windows.
	/// </summary>
	public static class WindowStateManager
	{
		static IWindowStateManager? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IWindowStateManager Default =>
			defaultImplementation ??= new WindowStateManagerImplementation();

		internal static void SetDefault(IWindowStateManager? implementation) =>
			defaultImplementation = implementation;
	}

	static class WindowStateManagerExtensions
	{
		/// <summary>
		/// Gets the application's currently active window.
		/// </summary>
		/// <param name="manager">The object to invoke this method on.</param>
		/// <param name="throwOnNull">Throws an exception if no current <see cref="Window"/> can be found and this value is set to <see langword="true"/>, otherwise this method returns <see langword="null"/>.</param>
		/// <returns>The application's currently active <see cref="Window"/> object.</returns>
		/// <exception cref="NullReferenceException">Thrown if no current <see cref="Window"/> can be found and <paramref name="throwOnNull"/> is set to <see langword="true"/>.</exception>
		public static Window? GetActiveWindow(this IWindowStateManager manager, bool throwOnNull)
		{
			var window = manager.GetActiveWindow();
			if (throwOnNull && window == null)
				throw new NullReferenceException("The active Window cannot be detected. Ensure that you have called Init in your Application class.");

			return window;
		}

		/// <summary>
		/// Gets the application's currently active window's pointer.
		/// </summary>
		/// <param name="manager">The object to invoke this method on.</param>
		/// <param name="throwOnNull">Throws an exception if no current <see cref="Window"/> can be found and this value is set to <see langword="true"/>, otherwise this method returns <see cref="IntPtr.Zero"/>.</param>
		/// <returns>The application's currently active window's <see cref="IntPtr"/>.</returns>
		/// <exception cref="NullReferenceException">Thrown if no current <see cref="Window"/> can be found and <paramref name="throwOnNull"/> is set to <see langword="true"/>.</exception>
		public static IntPtr GetActiveWindowHandle(this IWindowStateManager manager, bool throwOnNull)
		{
			var window = manager.GetActiveWindow();
			if (throwOnNull && window == null)
				throw new NullReferenceException("The active Window cannot be detected. Ensure that you have called Init in your Application class.");

			if (window == null)
				return IntPtr.Zero;

			var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);

			return handle;
		}

		/// <summary>
		/// Gets the application's currently active app window.
		/// </summary>
		/// <param name="manager">The object to invoke this method on.</param>
		/// <param name="throwOnNull">Throws an exception if no current <see cref="AppWindow"/> can be found and this value is set to <see langword="true"/>, otherwise this method returns <see langword="null"/>.</param>
		/// <returns>The application's currently active <see cref="AppWindow"/> object.</returns>
		/// <exception cref="NullReferenceException">Thrown if no current <see cref="AppWindow"/> can be found and <paramref name="throwOnNull"/> is set to <see langword="true"/>.</exception>
		public static AppWindow? GetActiveAppWindow(this IWindowStateManager manager, bool throwOnNull)
		{
			var window = manager.GetActiveWindow();
			if (throwOnNull && window == null)
				throw new NullReferenceException("The active Window cannot be detected. Ensure that you have called Init in your Application class.");

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

		public void OnPlatformWindowInitialized(Window window)
		{
			SetActiveWindow(window);
		}

		public void OnActivated(Window window, WindowActivatedEventArgs args)
		{
			if (args.WindowActivationState != WindowActivationState.Deactivated)
				SetActiveWindow(window);
		}
	}
}
