using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class UIApplicationExtensions
	{
		internal static UIEdgeInsets GetSafeAreaInsetsForWindow(this UIApplication application)
		{
			UIEdgeInsets safeAreaInsets;

			if (!OperatingSystem.IsIOSVersionAtLeast(11))
				safeAreaInsets = new UIEdgeInsets(UIApplication.SharedApplication.StatusBarFrame.Size.Height, 0, 0, 0);
#pragma warning disable CA1422 // Validate platform compatibility
			else if (application.GetKeyWindow() is UIWindow keyWindow)
				safeAreaInsets = keyWindow.SafeAreaInsets;
#pragma warning disable CA1416 // TODO: 'UIApplication.Windows' is unsupported on: 'ios' 15.0 and later.
			else if (application.Windows.Length > 0)
				safeAreaInsets = application.Windows[0].SafeAreaInsets;
#pragma warning restore CA1416
			else
				safeAreaInsets = UIEdgeInsets.Zero;
#pragma warning restore CA1422 // Validate platform compatibility

			return safeAreaInsets;
		}

		public static UIWindow? GetKeyWindow(this UIApplication application)
		{
#pragma warning disable CA1416 // TODO: 'UIApplication.Windows' is unsupported on: 'ios' 15.0 and later.
#pragma warning disable CA1422 // Validate platform compatibility
			var windows = application.Windows;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416

			for (int i = 0; i < windows.Length; i++)
			{
				var window = windows[i];
				if (window.IsKeyWindow)
					return window;
			}

			return null;
		}

		public static IWindow? GetWindow(this UIApplication application) =>
			application.GetKeyWindow().GetWindow();

		public static IWindow? GetWindow(this UIWindow? platformWindow)
		{
			if (platformWindow is null)
				return null;

			foreach (var window in MauiUIApplicationDelegate.Current.Application.Windows)
			{
				if (window?.Handler?.PlatformView == platformWindow)
					return window;
			}

			return null;
		}

		public static IWindow? GetWindow(this UIWindowScene? windowScene)
		{
			if (windowScene is null)
				return null;

#pragma warning disable CA1416 // TODO: 'UIApplication.Windows' is unsupported on: 'ios' 15.0 and later
			foreach (var window in windowScene.Windows)
			{
				var managedWindow = window.GetWindow();

				if (managedWindow is not null)
					return managedWindow;
			}
#pragma warning restore CA1416

			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return null;
			else if (windowScene.Delegate is IUIWindowSceneDelegate sd)
				return sd.GetWindow().GetWindow();

			return null;
		}
	}
}