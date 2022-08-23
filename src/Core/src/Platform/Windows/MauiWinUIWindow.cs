using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiWinUIWindow : UI.Xaml.Window
	{
		readonly WindowMessageManager _windowManager;

		IntPtr _windowIcon;
		bool _enableResumeEvent;

		public MauiWinUIWindow()
		{
			_windowManager = WindowMessageManager.Get(this);

			Activated += OnActivated;
			Closed += OnClosedPrivate;
			VisibilityChanged += OnVisibilityChanged;

			// We set this to true by default so later on if it's
			// set to false we know the user toggled this to false 
			// and then we can react accordingly
			ExtendsContentIntoTitleBar = true;

			SubClassingWin32();
			SetIcon();
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

		private void OnClosedPrivate(object sender, UI.Xaml.WindowEventArgs args)
		{
			OnClosed(sender, args);

			if (_windowIcon != IntPtr.Zero)
			{
				DestroyIcon(_windowIcon);
				_windowIcon = IntPtr.Zero;
			}
		}

		protected virtual void OnClosed(object sender, UI.Xaml.WindowEventArgs args)
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnClosed>(del => del(this, args));
		}

		protected virtual void OnVisibilityChanged(object sender, UI.Xaml.WindowVisibilityChangedEventArgs args)
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnVisibilityChanged>(del => del(this, args));
		}

		public IntPtr WindowHandle => _windowManager.WindowHandle;

		void SubClassingWin32()
		{
			MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnPlatformWindowSubclassed>(
				del => del(this, new WindowsPlatformWindowSubclassedEventArgs(WindowHandle)));

			_windowManager.WindowMessage += OnWindowMessage;

			void OnWindowMessage(object? sender, WindowMessageEventArgs e)
			{
				if (e.MessageId == PlatformMethods.MessageIds.WM_SETTINGCHANGE ||
					e.MessageId == PlatformMethods.MessageIds.WM_THEMECHANGE)
				{
					MauiWinUIApplication.Current.Application?.ThemeChanged();
				}
				else if (e.MessageId == PlatformMethods.MessageIds.WM_DPICHANGED)
				{
					var dpiX = (short)(long)e.WParam;
					var dpiY = (short)((long)e.WParam >> 16);

					var window = this.GetWindow();
					if (window is not null)
						window.DisplayDensityChanged(dpiX / DeviceDisplay.BaseLogicalDpi);
				}

				MauiWinUIApplication.Current.Services?.InvokeLifecycleEvents<WindowsLifecycle.OnPlatformMessage>(
					m => m.Invoke(this, new WindowsPlatformMessageEventArgs(e.Hwnd, e.MessageId, e.WParam, e.LParam)));
			}
		}

		/// <summary>
		/// Default the Window Icon to the icon stored in the .exe, if any.
		/// 
		/// The Icon can be overriden by callers by calling SetIcon themselves.
		/// </summary>
		void SetIcon()
		{
			var processPath = Environment.ProcessPath;
			if (!string.IsNullOrEmpty(processPath))
			{
				var index = IntPtr.Zero; // 0 = first icon in resources
				_windowIcon = ExtractAssociatedIcon(IntPtr.Zero, processPath, ref index);
				if (_windowIcon != IntPtr.Zero)
				{
					var appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowHandle));
					if (appWindow is not null)
					{
						var iconId = Win32Interop.GetIconIdFromIcon(_windowIcon);
						appWindow.SetIcon(iconId);
					}
				}
			}
		}

		UI.Xaml.UIElement? _customTitleBar;
		internal UI.Xaml.UIElement? MauiCustomTitleBar
		{
			get => _customTitleBar;
			set
			{
				_customTitleBar = value;
				SetTitleBar(_customTitleBar);
				UpdateTitleOnCustomTitleBar();
			}
		}

		internal void UpdateTitleOnCustomTitleBar()
		{
			if (_customTitleBar is UI.Xaml.FrameworkElement fe &&
				fe.GetDescendantByName<TextBlock>("AppTitle") is TextBlock tb)
			{
				tb.Text = Title;
			}
		}


		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, string iconPath, ref IntPtr index);

		[DllImport("user32.dll", SetLastError = true)]
		static extern int DestroyIcon(IntPtr hIcon);
	}
}
