using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Essentials;
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

		PlatformMethods.WindowProc? newWndProc = null;
		IntPtr oldWndProc = IntPtr.Zero;

		void SubClassingWin32()
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnPlatformWindowSubclassed>(
				del => del(this, new WindowsPlatformWindowSubclassedEventArgs(WindowHandle)));

			newWndProc = new PlatformMethods.WindowProc(NewWindowProc);
			oldWndProc = PlatformMethods.SetWindowLongPtr(WindowHandle, PlatformMethods.WindowLongFlags.GWL_WNDPROC, newWndProc);

			IntPtr NewWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
			{
				if (msg == WindowsPlatformMessageIds.WM_SETTINGCHANGE || msg == WindowsPlatformMessageIds.WM_THEMECHANGE)
					MauiWinUIApplication.Current.Application?.ThemeChanged();

				if (msg == WindowsPlatformMessageIds.WM_DPICHANGED)
				{
					var dpiX = (short)(long)wParam;
					var dpiY = (short)((long)wParam >> 16);

					var window = this.GetWindow();
					if (window is not null)
						window.DisplayDensityChanged(dpiX / DeviceDisplay.BaseLogicalDpi);
				}

				MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnPlatformMessage>(
					m => m.Invoke(this, new WindowsPlatformMessageEventArgs(hWnd, msg, wParam, lParam)));

				return PlatformMethods.CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
			}
		}

		#endregion
	}
}
