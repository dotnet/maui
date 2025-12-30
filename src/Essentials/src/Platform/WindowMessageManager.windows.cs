#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.ApplicationModel
{
	class WindowMessageManager : IDisposable
	{
		readonly static Dictionary<IntPtr, WeakReference<WindowMessageManager>> _managers = new();
		readonly static PlatformMethods.WindowProc _newWndProc = new(NewWindowProc);

		readonly object _locker = new();

		IntPtr _windowHandle;
		IntPtr _oldWndProc;

		bool _isDisposed;

		event EventHandler<WindowMessageEventArgs>? WindowMessageInternal;

		WindowMessageManager(Window window)
		{
			_windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
		}

		public IntPtr WindowHandle => _windowHandle;

		public bool IsAttached => _oldWndProc != IntPtr.Zero;

		public static IEnumerable<WindowMessageManager> GetAll()
		{
			foreach (var weakManager in _managers.Values.ToArray())
			{
				if (weakManager.TryGetTarget(out var manager))
					yield return manager;
			}
		}

		public event EventHandler<WindowMessageEventArgs> WindowMessage
		{
			add
			{
				if (WindowMessageInternal is null)
					Attach();

				WindowMessageInternal += value;
			}
			remove
			{
				WindowMessageInternal -= value;

				if (WindowMessageInternal is null)
					Detach();
			}
		}

		void Attach()
		{
			lock (_locker)
			{
				if (_oldWndProc == IntPtr.Zero)
				{
					_oldWndProc = PlatformMethods.SetWindowLongPtr(_windowHandle, PlatformMethods.WindowLongFlags.GWL_WNDPROC, _newWndProc);
				}
			}
		}

		void Detach()
		{
			lock (_locker)
			{
				if (_oldWndProc != IntPtr.Zero)
				{
					PlatformMethods.SetWindowLongPtr(_windowHandle, PlatformMethods.WindowLongFlags.GWL_WNDPROC, _oldWndProc);
					_oldWndProc = IntPtr.Zero;
				}
			}
		}

		static IntPtr NewWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
		{
			if (_managers.TryGetValue(hWnd, out var weakManager) && weakManager.TryGetTarget(out var manager))
			{
				var evt = manager.WindowMessageInternal;
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

		public static WindowMessageManager Get(Window window)
		{
			var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);

			if (_managers.TryGetValue(handle, out var weakManager) &&
				weakManager.TryGetTarget(out var manager) &&
				!manager._isDisposed)
				return manager;

			var newManager = new WindowMessageManager(window);

			_managers[handle] = new WeakReference<WindowMessageManager>(newManager);

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

				Detach();

				// set large fields to null

				_windowHandle = IntPtr.Zero;
				_oldWndProc = IntPtr.Zero;

				_isDisposed = true;
			}
		}

		~WindowMessageManager()
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
