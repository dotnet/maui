#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.ApplicationModel
{
	class WindowManager : IDisposable
	{
		readonly static Dictionary<IntPtr, WeakReference<WindowManager>> _managers = new();
		readonly static PlatformMethods.WindowProc _newWndProc = new(NewWindowProc);

		readonly IntPtr _windowHandle;
		readonly IntPtr _oldWndProc;

		bool _isDisposed;

		WindowManager(Window window)
		{
			_windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
			_oldWndProc = PlatformMethods.SetWindowLongPtr(_windowHandle, PlatformMethods.WindowLongFlags.GWL_WNDPROC, _newWndProc);
		}

		public IntPtr WindowHandle => _windowHandle;

		public event EventHandler<WindowMessageEventArgs>? WindowMessage;

		static IntPtr NewWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
		{
			if (_managers.TryGetValue(hWnd, out var weakManager) && weakManager.TryGetTarget(out var manager))
			{
				var evt = manager.WindowMessage;
				if (evt is not null)
				{
					var args = new WindowMessageEventArgs(hWnd, uMsg, wParam, lParam);

					evt.Invoke(manager, args);

					if (args.Handled)
						return args.Result;
				}

				return PlatformMethods.CallWindowProc(manager._oldWndProc, hWnd, uMsg, wParam, lParam);
			}

			// this technically should never happen
			return PlatformMethods.DefSubclassProc(hWnd, uMsg, wParam, lParam);
		}

		public static WindowManager Get(Window window)
		{
			var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);

			if (_managers.TryGetValue(handle, out var weakManager) &&
				weakManager.TryGetTarget(out var manager) &&
				!manager._isDisposed)
				return manager;

			var newManager = new WindowManager(window);

			_managers[handle] = new WeakReference<WindowManager>(newManager);

			return newManager;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)

					if (_managers.ContainsKey(_windowHandle))
						_managers.Remove(_windowHandle);
				}

				// free unmanaged resources (unmanaged objects) and override finalizer

				var newPtr = PlatformMethods.SetWindowLongPtr(_windowHandle, PlatformMethods.WindowLongFlags.GWL_WNDPROC, _oldWndProc);
				var p = Marshal.GetFunctionPointerForDelegate(_newWndProc);
				var same = p == newPtr;

				// set large fields to null

				_isDisposed = true;
			}
		}

		~WindowManager()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
