using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>The Win32 and GDK-Win32 calls the WebView2 host needs. Only called on Windows.</summary>
	internal static class GtkWin32
	{
		const string GtkLibrary = "gtk-4-1.dll";
		const uint EventObjectLocationChange = 0x800B;
		const uint WinEventOutOfContext = 0;
		const int ObjIdWindow = 0;

		delegate void WinEventProc(IntPtr hook, uint winEvent, IntPtr hwnd, int idObject, int idChild, uint thread, uint time);

		// GDK 4 on Windows has no separate gdk library: gdk_win32_surface_get_handle is exported by gtk-4-1.dll.
		[DllImport(GtkLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gdk_win32_surface_get_handle(IntPtr surface);

		[DllImport("user32.dll")]
		static extern IntPtr SetFocus(IntPtr hwnd);

		[DllImport("user32.dll")]
		static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr module, WinEventProc proc, uint processId, uint threadId, uint flags);

		[DllImport("user32.dll")]
		static extern bool UnhookWinEvent(IntPtr hook);

		[DllImport("kernel32.dll")]
		static extern uint GetCurrentThreadId();

		/// <summary>The HWND of a GDK surface, or <see cref="IntPtr.Zero"/> when it is not a Win32 surface.</summary>
		/// <param name="surface">The <c>GdkSurface*</c>.</param>
		internal static IntPtr GetSurfaceHandle(IntPtr surface) =>
			surface == IntPtr.Zero ? IntPtr.Zero : gdk_win32_surface_get_handle(surface);

		/// <summary>Gives Win32 keyboard focus to <paramref name="hwnd"/>.</summary>
		internal static void Focus(IntPtr hwnd) => SetFocus(hwnd);

		/// <summary>
		/// Calls <paramref name="moved"/> whenever <paramref name="hwnd"/> moves or resizes on the screen (<c>EVENT_OBJECT_LOCATIONCHANGE</c>,
		/// delivered through the calling thread's message pump, that is the GTK main loop). GDK 4 has no signal for a toplevel's position.
		/// </summary>
		/// <param name="hwnd">The window to watch.</param>
		/// <param name="moved">The callback.</param>
		/// <returns>The subscription; dispose it to unhook.</returns>
		internal static IDisposable WatchLocation(IntPtr hwnd, Action moved) => new LocationWatch(hwnd, moved);

		sealed class LocationWatch : IDisposable
		{
			readonly WinEventProc _proc;
			IntPtr _hook;

			public LocationWatch(IntPtr hwnd, Action moved)
			{
				_proc = (hook, winEvent, window, idObject, idChild, thread, time) =>
				{
					if (window != hwnd || idObject != ObjIdWindow)
						return;

					try
					{
						moved();
					}
					catch (Exception)
					{
						// A managed exception must not cross into Win32.
					}
				};
				_hook = SetWinEventHook(EventObjectLocationChange, EventObjectLocationChange, IntPtr.Zero, _proc, 0, GetCurrentThreadId(), WinEventOutOfContext);
			}

			public void Dispose()
			{
				var hook = _hook;
				_hook = IntPtr.Zero;
				if (hook != IntPtr.Zero)
					UnhookWinEvent(hook);
			}
		}
	}
}
