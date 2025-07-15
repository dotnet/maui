using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		internal static void UpdateTitle(this UIWindow platformWindow, IWindow window)
		{
			// If you set the title to null the app will crash
			// If you set it to an empty string the title reverts back to the 
			// default app title.
			if (OperatingSystem.IsIOSVersionAtLeast(13) && platformWindow.WindowScene is not null)
			{
				platformWindow.WindowScene.Title = window.Title ?? String.Empty;
			}
		}

		internal static void UpdateX(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateCoordinates(window);

		internal static void UpdateY(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateCoordinates(window);

		internal static void UpdateWidth(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateCoordinates(window);

		internal static void UpdateHeight(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateCoordinates(window);

		internal static void UpdateCoordinates(this UIWindow platformWindow, IWindow window)
		{
			if (OperatingSystem.IsMacCatalyst() && OperatingSystem.IsIOSVersionAtLeast(16) &&
			    platformWindow.WindowScene is { } windowScene)
			{
				if (double.IsNaN(window.X) || double.IsNaN(window.Y) || double.IsNaN(window.Width) ||
				    double.IsNaN(window.Height))
				{
					return;
				}

				var preferences = new UIWindowSceneGeometryPreferencesMac()
				{
					SystemFrame = new CGRect(window.X, window.Y, window.Width, window.Height)
				};

				windowScene.RequestGeometryUpdate(preferences, (error) =>
				{
					window.Handler?.MauiContext?.CreateLogger<UIWindow>()
						?.LogError("Requesting geometry update failed with error '{error}'.", error);
				});
			}
			else
			{
				window.FrameChanged(platformWindow.Bounds.ToRectangle());
			}
		}

		public static void UpdateMaximumWidth(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMaximumSize(window.MaximumWidth, window.MaximumHeight);

		public static void UpdateMaximumHeight(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMaximumSize(window.MaximumWidth, window.MaximumHeight);

		public static void UpdateMaximumSize(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMaximumSize(window.MaximumWidth, window.MaximumHeight);

		internal static void UpdateMaximumSize(this UIWindow platformWindow, double width, double height)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
			{
				return;
			}

			var restrictions = platformWindow.WindowScene?.SizeRestrictions;
			if (restrictions is null)
				return;

			if (!Primitives.Dimension.IsExplicitSet(width) || !Primitives.Dimension.IsMaximumSet(width))
				width = double.MaxValue;
			if (!Primitives.Dimension.IsExplicitSet(height) || !Primitives.Dimension.IsMaximumSet(height))
				height = double.MaxValue;

			restrictions.MaximumSize = new CoreGraphics.CGSize(width, height);
		}

		public static void UpdateMinimumWidth(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMinimumSize(window.MinimumWidth, window.MinimumHeight);

		public static void UpdateMinimumHeight(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMinimumSize(window.MinimumWidth, window.MinimumHeight);

		public static void UpdateMinimumSize(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMinimumSize(window.MinimumWidth, window.MinimumHeight);

#if MACCATALYST
		// See: https://gist.github.com/rolfbjarne/981b778a99425a6e630c
		const string LibobjcDylib = "/usr/lib/libobjc.dylib";
		
		[DllImport(LibobjcDylib, EntryPoint = "objc_msgSend")]
		static extern void void_objc_msgSend_bool(IntPtr receiver, IntPtr selector, bool arg1);
		
		[DllImport(LibobjcDylib, EntryPoint = "objc_msgSend")]
		internal static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);
		
		// Lazy-loaded selector for requesting a standard window button
		static Selector? StandardWindowButtonSelector;
		static Selector StandardWindowButton => StandardWindowButtonSelector ??= new Selector("standardWindowButton:");
		
		// Lazy-loaded selector for hiding/showing a button
		static Selector? StandardWindowButtonSetHiddenSelector;
		static Selector StandardWindowButtonSetHidden => StandardWindowButtonSetHiddenSelector ??= new Selector("setHidden:");
		
		enum NSWindowButton : ulong
		{
			MinimizeButton,
			MaximizeButton
		}

		public static async Task UpdateIsMinimizableAsync(this UIWindow platformWindow, IWindow window)
		{
			var nsWindow = await GetNSWindowFromUIWindowAsync(platformWindow);

			if (nsWindow != null)
			{
				// Get the minimize button from the window
				var minimizeButton = Runtime.GetNSObject(
					IntPtr_objc_msgSend_IntPtr(
						nsWindow.Handle,
						StandardWindowButton.Handle,
						(IntPtr)(ulong)NSWindowButton.MinimizeButton));

				if (minimizeButton is not null)
				{
					// Update the button's visibility based on the window's state
					bool isMinimizable = window.IsMinimizable;
					
					void_objc_msgSend_bool(
						minimizeButton.Handle,
						StandardWindowButtonSetHidden.Handle,
						!isMinimizable);
				}
			}
		}
		
		public static async Task UpdateIsMaximizableAsync(this UIWindow platformWindow, IWindow window)
		{
			var nsWindow = await GetNSWindowFromUIWindowAsync(platformWindow);

			if (nsWindow != null)
			{
				// Get the maximize button from the window
				var maximizeButton = Runtime.GetNSObject(
					IntPtr_objc_msgSend_IntPtr(
						nsWindow.Handle,
						StandardWindowButton.Handle,
						(IntPtr)(ulong)NSWindowButton.MaximizeButton));

				if (maximizeButton is not null)
				{
					// Update the button's visibility based on the window's state
					bool isMaximizable = window.IsMaximizable;
					
					void_objc_msgSend_bool(
						maximizeButton.Handle,
						StandardWindowButtonSetHidden.Handle,
						!isMaximizable);
				}
			}
		}

		static async Task<NSObject?> GetNSWindowFromUIWindowAsync(this UIWindow window)
		{
			if (window is null)
			{
				return null;
			}

			var nsApplication = Runtime.GetNSObject(Class.GetHandle("NSApplication"));
			if (nsApplication is null)
			{
				return null;
			}

			var sharedApplication = nsApplication.PerformSelector(new Selector("sharedApplication"));
			if (sharedApplication is null)
			{
				return null;
			}

			var applicationDelegate = sharedApplication.PerformSelector(new Selector("delegate"));
			if (applicationDelegate is null)
			{
				return null;
			}

			return await GetNSWindowAsync(window, applicationDelegate);
		}

		static async Task<NSObject?> GetNSWindowAsync(UIWindow window, NSObject applicationDelegate)
		{
			// Custom selector used to bridge host window with UIKit UIWindow
			var nsWindowHandle = IntPtr_objc_msgSend_IntPtr(applicationDelegate.Handle,
				Selector.GetHandle("hostWindowForUIWindow:"), window.Handle);
			var nsWindow = Runtime.GetNSObject<NSObject>(nsWindowHandle);
			
			if (nsWindow is null)
			{
				// Delay to allow async window resolution
				await Task.Delay(500);
				return await GetNSWindowAsync(window, applicationDelegate);
			}

			return nsWindow;
		}
#endif

		internal static void UpdateMinimumSize(this UIWindow platformWindow, double width, double height)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
			{
				return;
			}

			var restrictions = platformWindow.WindowScene?.SizeRestrictions;
			if (restrictions is null)
				return;

			if (!Primitives.Dimension.IsExplicitSet(width) || !Primitives.Dimension.IsMinimumSet(width))
				width = 0;
			if (!Primitives.Dimension.IsExplicitSet(height) || !Primitives.Dimension.IsMinimumSet(height))
				height = 0;

			restrictions.MinimumSize = new CoreGraphics.CGSize(width, height);
		}

		internal static IWindow? GetHostedWindow(this UIWindow? uiWindow)
		{
			if (uiWindow is null)
				return null;

			var windows = WindowExtensions.GetWindows();
			foreach (var window in windows)
			{

				if (window.Handler?.PlatformView is UIWindow win)
				{
					if (win == uiWindow)
						return window;
				}
			}

			return null;
		}

		public static float GetDisplayDensity(this UIWindow uiWindow) =>
			(float)(uiWindow.Screen?.Scale ?? new nfloat(1.0f));

		internal static DisplayOrientation GetOrientation(this IWindow? window) =>
			DeviceDisplay.Current.MainDisplayInfo.Orientation;
	}
}