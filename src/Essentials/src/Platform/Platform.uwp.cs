#if WINDOWS_UWP
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WindowActivationState = Windows.UI.Core.CoreWindowActivationState;
#elif WINDOWS
using System;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
#endif

namespace Microsoft.Maui.Essentials
{
	public static partial class Platform
	{
		const uint DISPLAY_CHANGED = 126;
		const uint DPI_CHANGED = 736;

		static internal event EventHandler CurrentWindowChanged;
		static internal event EventHandler CurrentWindowDisplayChanged;

		internal static Window CurrentWindow
		{
			get => _currentWindow ?? Window.Current;
			set
			{
				bool changed = _currentWindow != value;
				_currentWindow = value;

				if(changed)
					CurrentWindowChanged?.Invoke(value, EventArgs.Empty);
			}
		}

		internal static IntPtr CurrentWindowHandle
			=> WinRT.Interop.WindowNative.GetWindowHandle(CurrentWindow);
#if WINDOWS
		internal static UI.WindowId CurrentWindowId
			=> UI.Win32Interop.GetWindowIdFromWindow(CurrentWindowHandle);

		internal static AppWindow CurrentAppWindow
			=> AppWindow.GetFromWindowId(CurrentWindowId);

		// Currently there isn't a way to detect Orientation Changes unless you subclass the WinUI.Window and watch the messages
		// Maui.Core forwards these messages to here so that WinUI can react accordingly.
		// This is the "subtlest" way to currently wire this together. 
		// Hopefully there will be a more public API for this down the road so we can just use that directly from Essentials
		internal static void NewWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			if (CurrentWindowDisplayChanged == null)
				return;

			// We only care about orientation or dpi changes
			if (DISPLAY_CHANGED != msg && DPI_CHANGED != msg)
				return;

			if (_currentWindow != null && hWnd == CurrentWindowHandle)
			{
				CurrentWindowDisplayChanged?.Invoke(CurrentWindow, EventArgs.Empty);
			}
		}
#endif

		internal const string AppManifestFilename = "AppxManifest.xml";
		internal const string AppManifestXmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
		internal const string AppManifestUapXmlns = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
		private static Window _currentWindow;

		public static string MapServiceToken { get; set; }

		public static async void OnLaunched(LaunchActivatedEventArgs e)
			=> await AppActions.OnLaunched(e);

		public static void OnActivated(Window window, WindowActivatedEventArgs args)
		{
			if (args.WindowActivationState != WindowActivationState.Deactivated)
			{
				CurrentWindow = window;
			}
		}
	}
}
