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
		public IntPtr WindowHandle => _hwnd;
		
		delegate IntPtr WinProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
		WinProc? newWndProc = null;
		IntPtr oldWndProc = IntPtr.Zero;

		[DllImport("user32")]
		static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WinProc newProc);
		[DllImport("user32.dll")]
		static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		void SubClassingWin32()
		{
			//Get the Window's HWND
			_hwnd = this.As<IWindowNative>().WindowHandle;
			if (_hwnd == IntPtr.Zero)
				throw new NullReferenceException("The Window Handle is null.");

			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnNativeWindowSubclassed>(
				del => del(this, new WindowsNativeWindowSubclassedEventArgs(_hwnd)));

			newWndProc = new WinProc(NewWindowProc);
			oldWndProc = SetWindowLongPtr(_hwnd, /* GWL_WNDPROC */ -4, newWndProc);
		}

		IntPtr NewWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			if (msg == WindowsNativeMessageIds.WM_SETTINGCHANGE || msg == WindowsNativeMessageIds.WM_THEMECHANGE)
				MauiWinUIApplication.Current.Application?.ThemeChanged();

			var args = new WindowsNativeMessageEventArgs(hWnd, msg, wParam, lParam);

			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnNativeMessage>(m => m(this, args));

			return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
		}

		#endregion
	}
}
