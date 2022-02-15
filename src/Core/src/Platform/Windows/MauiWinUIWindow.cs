using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.LifecycleEvents;
using WinRT;

namespace Microsoft.Maui
{
	public class MauiWinUIWindow : UI.Xaml.Window
	{
		bool _enableResumeEvent;
		public MauiWinUIWindow()
		{
			Activated += OnActivated;
			Closed += OnClosed;
			VisibilityChanged += OnVisibilityChanged;

			SubClassingWin32();
		}

		protected virtual void OnActivated(object sender, UI.Xaml.WindowActivatedEventArgs args)
		{
			if (args.WindowActivationState != UI.Xaml.WindowActivationState.Deactivated)
			{
				if (_enableResumeEvent)
					MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnResumed>(del => del(this));
				else
					_enableResumeEvent = true;
			}

			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnActivated>(del => del(this, args));
		}

		protected virtual void OnClosed(object sender, UI.Xaml.WindowEventArgs args)
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnClosed>(del => del(this, args));
		}

		protected virtual void OnVisibilityChanged(object sender, UI.Xaml.WindowVisibilityChangedEventArgs args)
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnVisibilityChanged>(del => del(this, args));
		}

		#region Native Window

		IntPtr _hwnd = IntPtr.Zero;

		/// <summary>
		/// Returns a pointer to the underlying platform window handle (hWnd).
		/// </summary>
		public IntPtr WindowHandle
		{
			get
			{
				if (_hwnd == IntPtr.Zero)
					_hwnd = this.GetWindowHandle();
				return _hwnd;
			}
		}

		NativeMethods.WindowProc? newWndProc = null;
		IntPtr oldWndProc = IntPtr.Zero;

		void SubClassingWin32()
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnNativeWindowSubclassed>(
				del => del(this, new WindowsNativeWindowSubclassedEventArgs(WindowHandle)));

			newWndProc = new NativeMethods.WindowProc(NewWindowProc);
			oldWndProc = NativeMethods.SetWindowLongPtr(WindowHandle, NativeMethods.WindowLongFlags.GWL_WNDPROC, newWndProc);

			IntPtr NewWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
			{
				if (msg == WindowsNativeMessageIds.WM_SETTINGCHANGE || msg == WindowsNativeMessageIds.WM_THEMECHANGE)
					MauiWinUIApplication.Current.Application?.ThemeChanged();

				MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnNativeMessage>(
					m => m.Invoke(this, new WindowsNativeMessageEventArgs(hWnd, msg, wParam, lParam)));

				return NativeMethods.CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
			}
		}

		#endregion
	}
}
